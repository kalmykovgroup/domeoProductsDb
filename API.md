# DomeoProductsDb — API Documentation

PIM-микросервис для конфигуратора корпусной мебели. Возвращает каталог (категории → товары → типизированные атрибуты → предложения поставщиков).

---

## 1. Архитектура

```
   клиент (UI / интегратор)
          │
          ▼
  ┌───────────────────┐        ┌────────────────────────┐       ┌──────────────┐
  │  ApiGateway       │───────▶│  ProductsService (API) │──────▶│  PostgreSQL  │
  │  (YARP, :5050)    │  /api/*│  (.NET 10, :5221)      │       │  (:5433)     │
  └───────────────────┘        └────────────────────────┘       └──────────────┘
```

- Все клиентские запросы идут через **gateway** (`http://localhost:5050`). ProductsService напрямую (`:5221`) доступен только для отладки.
- Gateway прозрачно проксирует `/api/**` и `/openapi/**` в ProductsService.
- Данные в БД засеваются из `seed.json` (1714 товаров, 48 категорий) при первом запуске.

### Порты (docker-compose)

| Компонент | Host | Container |
|---|---|---|
| ApiGateway | `5050` | `8080` |
| ProductsService | `5221` | `8080` |
| PostgreSQL | `5433` | `5432` |

### Базовый URL

- Dev (local docker): `http://localhost:5050`
- Internal service URL (внутри docker-сети): `http://products-service:8080`

---

## 2. Соглашения

### Формат

- Все запросы и ответы — **JSON, UTF-8**.
- Content-Type ответа: `application/json; charset=utf-8`.
- Имена полей в JSON — `camelCase` (C# `PascalCase` автоматически приводится).
- Enum-значения сериализуются как **строки в camelCase** (например `"enum"`, `"numeric"`, не `0`/`1`).

### Культура и единицы

- Денежные значения — `decimal`, валюта всегда `RUB`.
- Размеры — миллиметры (`width_mm`, `height_mm`, `thickness_mm`).
- Листовые материалы — м², кромка — погонные метры. Единица берётся из атрибута `Единица измерения`.
- Тексты — русский (`TitleRu`, `NameRu`).

### Картинки товара

Картинки хранятся локально (скачаны один раз из внешних источников `cdn.mdm-complect.ru` и `whatswood.ru`) и отдаются сервисом как статические файлы.

| Размер | Путь | Примерный вес | Где приходит |
|---|---|---|---|
| **Preview** | `/images/preview/{hash}.jpg` | 300×300, 5–15 KB | `ProductSummaryDto.previewUrl` (список), `ProductDetailDto.previewUrl` (карточка) |
| **Full** | `/images/full/{hash}.jpg` | оригинал (~30–80 KB) | `ProductDetailDto.fullUrl` (только карточка) |

- **Путь относительный** — префикс `/images/...` клиент склеивает с базовым URL API (`http://localhost:5050` для dev).
- **`{hash}`** — sha1 от оригинального URL, первые 16 hex-символов. Стабильно между пере-сидами.
- Если у товара нет картинки → `previewUrl` и `fullUrl` будут `null`.
- 404 при запросе несуществующего файла.
- Атрибут `Основное изображение` из `ProductDetailDto.attributes[]` намеренно **удалён** — картинки доступны только через типизированные поля `previewUrl` / `fullUrl`.

### Ошибки

Стандартные HTTP-коды:

| Код | Когда |
|---|---|
| `200 OK` | Успех |
| `400 Bad Request` | Неверные параметры (тип / отсутствующие) |
| `404 Not Found` | Ресурс не найден (например `GET /api/products/99999`) |
| `500 Internal Server Error` | Неожиданная ошибка сервиса |

Тело при ошибках — стандартный **RFC 7807 `application/problem+json`**:

```json
{
  "type":   "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title":  "Not Found",
  "status": 404,
  "traceId": "00-abc...xyz-01"
}
```

### Служебные эндпоинты

| Метод | Путь | Описание |
|---|---|---|
| `GET`  | `/health` | Health-check (gateway и service). Возвращает `{"status":"ok"}` |
| `GET`  | `/openapi/v1.json` | OpenAPI 3.1 спецификация |

---

## 3. Доменная модель

```
Category (дерево)
   └── Product (1714 шт)
          ├── SupplierOffer[]  — цены от поставщиков
          └── ProductAttributeValue[] — атрибуты c типами:
                 ├─ text      → строка
                 ├─ numeric   → decimal
                 ├─ bool      → true/false
                 ├─ enum      → ссылка на EnumValue (допустимое значение из справочника)
                 └─ brand     → ссылка на Brand

Справочники:
   Brand        (28)   Supplier    (17)
   ProductAttribute (101)   EnumValue (129, привязан к attribute)
```

### `AttributeValueType` (дискриминатор)

| Значение | Что в DTO заполнено |
|---|---|
| `text` | `valueText` |
| `numeric` | `valueNumeric` |
| `bool` | `valueBool` |
| `enum` | `enumValueId` + `enumCode` + `enumTitleRu` |
| `brand` | `brandId` + `brandTitleRu` |

На уровне БД это гарантировано CHECK-констрейнтом `ck_pav_type_exclusive`: ровно одно поле-значение заполнено, и его тип совпадает с `valueType`.

---

## 4. Эндпоинты

### 4.1 `GET /api/categories`

Плоский список всех категорий (без вложенности).

**Параметры:** нет.

**Ответ:** `200 OK`, `CategoryDto[]`.

```bash
curl http://localhost:5050/api/categories
```

```json
[
  {
    "id": 132,
    "parentId": 369,
    "code": "drawer_organizers_universal",
    "titleRu": "Органайзеры для ящиков (универсальные)",
    "isLeaf": true
  },
  {
    "id": 134,
    "parentId": 366,
    "code": "ldsp",
    "titleRu": "ЛДСП",
    "isLeaf": true
  }
]
```

---

### 4.2 `GET /api/categories/tree`

Иерархическое дерево категорий (корень → группы → листовые).

**Параметры:** нет.

**Ответ:** `200 OK`, `CategoryNode[]`. Массив корней; каждый узел содержит `children[]`. У листовых (`isLeaf=true`) есть продукты; у групп (`isLeaf=false`) — только дочерние категории.

```bash
curl http://localhost:5050/api/categories/tree
```

```json
[
  {
    "id": 363,
    "code": "custom_cabinetry",
    "titleRu": "Корпусная мебель на заказ",
    "isLeaf": false,
    "children": [
      {
        "id": 367,
        "code": "edge_banding_custom",
        "titleRu": "Кромка (п.м.)",
        "isLeaf": true,
        "children": []
      },
      { "id": 364, "code": "cabinet_modules", "titleRu": "Модули корпусной мебели", "isLeaf": true, "children": [] }
    ]
  }
]
```

---

### 4.3 `GET /api/products`

Поиск / листинг товаров с пагинацией и фильтрами.

**Query-параметры:**

| Имя | Тип | По умолчанию | Описание |
|---|---|---|---|
| `categoryId` | `int?` | — | Фильтр по листовой категории (id из `/api/categories`) |
| `q` | `string?` | — | Подстрочный поиск по `nameRu` или `externalCode` (case-insensitive) |
| `page` | `int` | `1` | Номер страницы, от 1. Меньшие значения нормализуются к 1 |
| `pageSize` | `int` | `20` | Размер страницы. Клэмп: от 1 до 200 |

**Ответ:** `200 OK`, `PagedResult<ProductSummaryDto>`.

```bash
curl "http://localhost:5050/api/products?categoryId=134&pageSize=2"
```

```json
{
  "items": [
    {
      "id": 611,
      "externalCode": "ldsp-egg-h1137-16-0019",
      "nameRu": "Egger ЛДСП H1137 ST12 Дуб Сорано чёрно-коричневый 16мм",
      "categoryId": 134,
      "categoryTitle": "ЛДСП",
      "minPrice": 3620.11,
      "mainSupplier": "Egger Россия",
      "previewUrl": "/images/preview/56c7b0b06f2b78d6.jpg"
    },
    {
      "id": 612,
      "externalCode": "ldsp-egg-h1137-18-0021",
      "nameRu": "Egger ЛДСП H1137 ST12 Дуб Сорано чёрно-коричневый 18мм",
      "categoryId": 134,
      "categoryTitle": "ЛДСП",
      "minPrice": 4267.03,
      "mainSupplier": "Egger Россия",
      "previewUrl": "/images/preview/56c7b0b06f2b78d6.jpg"
    }
  ],
  "total": 90,
  "page": 1,
  "pageSize": 2
}
```

**Примечания:**

- `minPrice` — минимальная цена из всех `Offers` товара. `null`, если оферов нет.
- `mainSupplier` — имя поставщика с минимальной ценой. `null`, если оферов нет.
- Сортировка стабильная по `id` (детали в разделе [Ограничения](#ограничения)).

---

### 4.4 `GET /api/products/{id}`

Полная карточка товара: атрибуты + все предложения поставщиков.

**Path-параметры:**

| Имя | Тип | Описание |
|---|---|---|
| `id` | `int` | `Product.Id` |

**Ответ:**

- `200 OK`, `ProductDetailDto`
- `404 Not Found`, если товар не существует

```bash
curl http://localhost:5050/api/products/1
```

```json
{
  "id": 1,
  "externalCode": "waste_so-blu-300-0138",
  "nameRu": "Blum Ведро TANDEMBOX 300мм",
  "categoryId": 187,
  "categoryTitle": "Аксессуары для систем сортировки",
  "previewUrl": "/images/preview/d986f3b0d6fb1e08.jpg",
  "fullUrl":    "/images/full/d986f3b0d6fb1e08.jpg",
  "offers": [
    { "supplierId": 1, "supplierName": "МДМ-Комплект", "priceAmount": 2750.0, "currency": "RUB" }
  ],
  "attributes": [
    {
      "attributeId": 15, "code": "артикул_производителя", "titleRu": "Артикул производителя",
      "valueType": "text",
      "valueText": "waste_so-blu-300-0138",
      "valueNumeric": null, "valueBool": null,
      "enumValueId": null, "enumCode": null, "enumTitleRu": null,
      "brandId": null, "brandTitleRu": null
    },
    {
      "attributeId": 3, "code": "бренд", "titleRu": "Бренд",
      "valueType": "brand",
      "brandId": 1, "brandTitleRu": "Blum",
      "valueText": null, "valueNumeric": null, "valueBool": null,
      "enumValueId": null, "enumCode": null, "enumTitleRu": null
    },
    {
      "attributeId": 10, "code": "высота_мм", "titleRu": "Высота (мм)",
      "valueType": "numeric",
      "valueNumeric": 500.0,
      "valueText": null, "valueBool": null,
      "enumValueId": null, "enumCode": null, "enumTitleRu": null,
      "brandId": null, "brandTitleRu": null
    },
    {
      "attributeId": 23, "code": "единица_измерения", "titleRu": "Единица измерения",
      "valueType": "enum",
      "enumValueId": 52, "enumCode": "штука", "enumTitleRu": "Штука",
      "valueText": null, "valueNumeric": null, "valueBool": null,
      "brandId": null, "brandTitleRu": null
    },
    {
      "attributeId": 9, "code": "плавное_закрывание", "titleRu": "Плавное закрывание",
      "valueType": "bool",
      "valueBool": true,
      "valueText": null, "valueNumeric": null,
      "enumValueId": null, "enumCode": null, "enumTitleRu": null,
      "brandId": null, "brandTitleRu": null
    }
  ]
}
```

**Примечания:**

- `offers` отсортированы по возрастанию `priceAmount`.
- `attributes` отсортированы по `titleRu` (алфавитный порядок атрибутов).
- Все 6 заполненных полей значения **взаимоисключаемы** — клиент должен смотреть на `valueType` и читать только соответствующее поле.

---

### 4.5 `GET /api/suppliers`

Список всех поставщиков (компактная форма, без контактов).

**Параметры:** нет.

**Ответ:** `200 OK`, `SupplierDto[]`. Отсортированы по `name`.

```bash
curl http://localhost:5050/api/suppliers
```

```json
[
  { "id": 12, "name": "AGT" },
  { "id": 13, "name": "Alvic" },
  { "id": 4,  "name": "Döllken" }
]
```

---

### 4.6 `GET /api/suppliers/{id}`

Детальная карточка поставщика с контактной информацией.

**Path-параметры:** `id` — `Supplier.Id`.

**Ответ:**
- `200 OK`, `SupplierDetailDto`
- `404 Not Found`

```bash
curl http://localhost:5050/api/suppliers/1
```

```json
{
  "id": 1,
  "name": "МДМ-Комплект",
  "email": null,
  "phone": null,
  "website": null,
  "address": null,
  "country": null,
  "inn": null
}
```

**Примечание:** схема данных содержит контактные поля (`email`, `phone`, `website`, `address`, `country`, `inn`), но в исходном PIM3-экспорте их нет. В тестовых данных они всегда `null`. Поля готовы к заполнению, когда подключат реальный источник контактов.

---

### 4.7 `GET /api/brands`

Список всех брендов. Используется, например, для фильтра «бренд» в каталоге.

**Параметры:** нет.

**Ответ:** `200 OK`, `BrandDto[]`. Отсортированы по `titleRu`.

```bash
curl http://localhost:5050/api/brands
```

```json
[
  { "id": 24, "titleRu": "AGT" },
  { "id": 19, "titleRu": "ARISTO" },
  { "id": 25, "titleRu": "Alvic" }
]
```

---

### 4.8 `GET /api/attributes`

Список всех атрибутов (метаданные каталога). Нужен для рендера фильтров / динамического формирования карточки товара.

**Параметры:** нет.

**Ответ:** `200 OK`, `AttributeDto[]`. Отсортированы по `titleRu`.

```bash
curl http://localhost:5050/api/attributes
```

```json
[
  { "id": 37, "code": "push_to_open",          "titleRu": "Push-to-Open",          "valueType": "bool"    },
  { "id": 15, "code": "артикул_производителя", "titleRu": "Артикул производителя", "valueType": "text"    },
  { "id": 3,  "code": "бренд",                 "titleRu": "Бренд",                 "valueType": "brand"   },
  { "id": 10, "code": "высота_мм",             "titleRu": "Высота (мм)",           "valueType": "numeric" },
  { "id": 36, "code": "страна_сборки",         "titleRu": "Страна сборки",         "valueType": "enum"    }
]
```

Текущая разбивка: 43 enum, 35 numeric, 17 bool, 5 text, 1 brand = **101 атрибут**.

---

### 4.9 `GET /api/attributes/{id}/enum-values`

Допустимые значения enum-атрибута — то, что рисуется в dropdown-селекте.

**Path-параметры:** `id` — `ProductAttribute.Id`.

**Ответ:**
- `200 OK`, `EnumValueDto[]` — для enum-атрибута с его значениями, отсортированными по `sortOrder`
- `200 OK`, `[]` — для атрибута, существующего, но **не enum** (numeric/bool/text/brand)
- `404 Not Found` — атрибут не найден

```bash
curl http://localhost:5050/api/attributes/36/enum-values
```

```json
[
  { "id": 66, "attributeId": 36, "code": "австрия",        "titleRu": "Австрия",        "sortOrder": 0 },
  { "id": 67, "attributeId": 36, "code": "великобритания", "titleRu": "Великобритания", "sortOrder": 1 },
  { "id": 68, "attributeId": 36, "code": "германия",       "titleRu": "Германия",       "sortOrder": 2 },
  { "id": 69, "attributeId": 36, "code": "италия",         "titleRu": "Италия",         "sortOrder": 3 },
  { "id": 70, "attributeId": 36, "code": "китай",          "titleRu": "Китай",          "sortOrder": 4 },
  { "id": 71, "attributeId": 36, "code": "польша",         "titleRu": "Польша",         "sortOrder": 5 },
  { "id": 72, "attributeId": 36, "code": "россия",         "titleRu": "Россия",         "sortOrder": 6 },
  { "id": 73, "attributeId": 36, "code": "турция",         "titleRu": "Турция",         "sortOrder": 7 },
  { "id": 74, "attributeId": 36, "code": "финляндия",      "titleRu": "Финляндия",      "sortOrder": 8 }
]
```

**Для какого атрибута какие значения:**
```sql
-- атрибуты с количеством enum-значений
SELECT "Id", "Code", "TitleRu" FROM reference.attributes WHERE "ValueType" = 'Enum';
```

---

## 5. DTO Reference

Все типы — неизменяемые `record`-ы, сериализованные в camelCase.

### `CategoryDto`

Плоское представление категории.

| Поле | Тип | Nullable | Описание |
|---|---|---|---|
| `id` | `int` | нет | Primary key |
| `parentId` | `int?` | да | FK на родительскую категорию. `null` у корня |
| `code` | `string` | нет | Технический код (уникален), напр. `"ldsp"`, `"facade_mdf_painted"` |
| `titleRu` | `string` | нет | Название на русском |
| `isLeaf` | `bool` | нет | `true` — содержит товары. `false` — группирующая категория |

### `CategoryNode`

Узел дерева. Отличается от `CategoryDto` отсутствием `parentId` (связь выражена вложенностью) и массивом `children`.

| Поле | Тип | Nullable | Описание |
|---|---|---|---|
| `id`, `code`, `titleRu`, `isLeaf` | см. выше | | |
| `children` | `CategoryNode[]` | нет | Дочерние узлы. Пустой массив у листов |

### `ProductSummaryDto`

Строка в листинге / результатах поиска. Лёгкая версия товара без атрибутов.

| Поле | Тип | Nullable | Описание |
|---|---|---|---|
| `id` | `int` | нет | Primary key |
| `externalCode` | `string` | нет | Уникальный артикул, напр. `"hinge-71B35-nickel-0001"` |
| `nameRu` | `string` | нет | Название |
| `categoryId` | `int` | нет | FK на листовую категорию |
| `categoryTitle` | `string` | нет | `Category.TitleRu` — избегает доп. запроса |
| `minPrice` | `decimal?` | да | Наименьшая цена из оферов. `null`, если оферов нет |
| `mainSupplier` | `string?` | да | Имя поставщика с минимальной ценой |
| `previewUrl` | `string?` | да | Относительный путь к превью 300×300, напр. `"/images/preview/d986f3b0d6fb1e08.jpg"`. `null`, если картинки нет |

### `ProductDetailDto`

Полная карточка.

| Поле | Тип | Nullable | Описание |
|---|---|---|---|
| `id`, `externalCode`, `nameRu`, `categoryId`, `categoryTitle` | см. `ProductSummaryDto` | | |
| `previewUrl` | `string?` | да | Как в `ProductSummaryDto.previewUrl` |
| `fullUrl` | `string?` | да | Полноразмерная картинка, напр. `"/images/full/d986f3b0d6fb1e08.jpg"` |
| `offers` | `OfferDto[]` | нет | Все предложения по возрастанию цены |
| `attributes` | `AttributeValueDto[]` | нет | Все атрибуты, отсортированы по `titleRu`. Атрибут «Основное изображение» сюда **не попадает** — картинки доступны только через `previewUrl`/`fullUrl` |

### `OfferDto`

Предложение одного поставщика.

| Поле | Тип | Описание |
|---|---|---|
| `supplierId` | `int` | FK |
| `supplierName` | `string` | `Supplier.Name` |
| `priceAmount` | `decimal` | Цена. Всегда > 0 |
| `currency` | `string` | ISO-4217, сейчас всегда `"RUB"` |

### `AttributeValueDto`

Одно значение одного атрибута у одного товара. Типизировано через дискриминатор.

| Поле | Тип | Nullable | Описание |
|---|---|---|---|
| `attributeId` | `int` | нет | FK на `reference.attributes` |
| `code` | `string` | нет | Код атрибута (slug), напр. `"страна_сборки"` |
| `titleRu` | `string` | нет | Название атрибута |
| `valueType` | `AttributeValueType` | нет | Дискриминатор: `text` / `numeric` / `bool` / `enum` / `brand` |
| `valueText` | `string?` | да | Заполнено ⇔ `valueType = "text"` |
| `valueNumeric` | `decimal?` | да | Заполнено ⇔ `valueType = "numeric"` |
| `valueBool` | `bool?` | да | Заполнено ⇔ `valueType = "bool"` |
| `enumValueId` | `int?` | да | FK на `reference.enum_values`. Заполнено ⇔ `valueType = "enum"` |
| `enumCode` | `string?` | да | Slug enum-значения, напр. `"германия"` |
| `enumTitleRu` | `string?` | да | Человеческое название, напр. `"Германия"` |
| `brandId` | `int?` | да | FK на `reference.brands`. Заполнено ⇔ `valueType = "brand"` |
| `brandTitleRu` | `string?` | да | Название бренда, напр. `"Blum"` |

### `AttributeValueType` (enum)

```
"text"      — свободный текст
"numeric"   — число (decimal)
"bool"      — да/нет
"enum"      — значение из справочника reference.enum_values
"brand"     — ссылка на reference.brands
```

### `PagedResult<T>`

Обёртка пагинации. Используется в `GET /api/products`.

| Поле | Тип | Описание |
|---|---|---|
| `items` | `T[]` | Страница данных |
| `total` | `int` | Общее количество подходящих записей (не только на текущей странице) |
| `page` | `int` | Номер текущей страницы, от 1 |
| `pageSize` | `int` | Фактический размер страницы после клэмпа (1..200) |

### `SupplierDto`

Компактная форма для списков.

| Поле | Тип | Описание |
|---|---|---|
| `id` | `int` | PK |
| `name` | `string` | Имя поставщика |

### `SupplierDetailDto`

Полная карточка. Используется в `GET /api/suppliers/{id}`.

| Поле | Тип | Nullable | Описание |
|---|---|---|---|
| `id` | `int` | нет | PK |
| `name` | `string` | нет | Имя поставщика |
| `email` | `string?` | да | Контактный email. Сейчас всегда `null` |
| `phone` | `string?` | да | Телефон. Сейчас всегда `null` |
| `website` | `string?` | да | URL сайта. Сейчас всегда `null` |
| `address` | `string?` | да | Почтовый/фактический адрес. Сейчас всегда `null` |
| `country` | `string?` | да | Страна. Сейчас всегда `null` |
| `inn` | `string?` | да | ИНН. Сейчас всегда `null` |

### `BrandDto`

| Поле | Тип | Описание |
|---|---|---|
| `id` | `int` | PK |
| `titleRu` | `string` | Название бренда. Например `"Blum"`, `"Hettich"` |

### `AttributeDto`

Метаданные атрибута (справочник).

| Поле | Тип | Описание |
|---|---|---|
| `id` | `int` | PK — совпадает с `AttributeValueDto.attributeId` |
| `code` | `string` | Slug-идентификатор, например `"страна_сборки"` |
| `titleRu` | `string` | Человеческое название |
| `valueType` | `AttributeValueType` | Тип значения: `text` / `numeric` / `bool` / `enum` / `brand`. Определяет какое поле `AttributeValueDto` будет заполнено для этого атрибута у товара |

### `EnumValueDto`

Допустимое значение enum-атрибута.

| Поле | Тип | Описание |
|---|---|---|
| `id` | `int` | PK |
| `attributeId` | `int` | FK на атрибут, к которому принадлежит значение |
| `code` | `string` | Slug, напр. `"германия"` |
| `titleRu` | `string` | Человеческое название, напр. `"Германия"` |
| `sortOrder` | `int` | Порядок для UI dropdown (от 0) |

---

## 6. Рецепты (частые сценарии)

### Построить tree view категорий в UI

```js
const tree = await fetch('http://localhost:5050/api/categories/tree').then(r => r.json());
// tree[0] = корень custom_cabinetry. Рекурсивно обходить .children
```

### Показать товары одной категории + ценник + превью

```js
const API = 'http://localhost:5050';
const page = 1;
const { items, total } = await fetch(
  `${API}/api/products?categoryId=134&page=${page}&pageSize=50`
).then(r => r.json());

for (const p of items) {
  const img = p.previewUrl ? `<img src="${API}${p.previewUrl}" width="150">` : '';
  document.body.insertAdjacentHTML('beforeend',
    `<div>${img}<br>${p.nameRu}<br>${p.minPrice} ₽ — ${p.mainSupplier}</div>`);
}
```

### Открыть полноразмерную картинку товара

```js
const API = 'http://localhost:5050';
const p = await fetch(`${API}/api/products/1`).then(r => r.json());
if (p.fullUrl) window.open(`${API}${p.fullUrl}`);
```

### Поиск по артикулу или названию

```js
const query = encodeURIComponent('Blum');
const res = await fetch(
  `http://localhost:5050/api/products?q=${query}&pageSize=20`
).then(r => r.json());
```

### Отрендерить карточку товара с типобезопасными атрибутами (TypeScript)

```ts
type AttributeValueType = 'text' | 'numeric' | 'bool' | 'enum' | 'brand';

interface AttributeValueDto {
  attributeId: number;
  code: string;
  titleRu: string;
  valueType: AttributeValueType;
  valueText:    string  | null;
  valueNumeric: number  | null;
  valueBool:    boolean | null;
  enumValueId:  number  | null;
  enumCode:     string  | null;
  enumTitleRu:  string  | null;
  brandId:      number  | null;
  brandTitleRu: string  | null;
}

function renderAttribute(a: AttributeValueDto): string {
  switch (a.valueType) {
    case 'text':    return `${a.titleRu}: ${a.valueText}`;
    case 'numeric': return `${a.titleRu}: ${a.valueNumeric}`;
    case 'bool':    return `${a.titleRu}: ${a.valueBool ? 'да' : 'нет'}`;
    case 'enum':    return `${a.titleRu}: ${a.enumTitleRu}`;
    case 'brand':   return `${a.titleRu}: ${a.brandTitleRu}`;
  }
}
```

### Построить dropdown допустимых значений для enum-атрибута

```js
// 1) Получили список атрибутов — нашли нужный
const attrs = await fetch('http://localhost:5050/api/attributes').then(r => r.json());
const country = attrs.find(a => a.code === 'страна_сборки');  // { id: 36, valueType: "enum", ... }

// 2) Получили список значений
const options = await fetch(
  `http://localhost:5050/api/attributes/${country.id}/enum-values`
).then(r => r.json());

// 3) Отрендерили dropdown
options.forEach(o => console.log(`<option value="${o.id}">${o.titleRu}</option>`));
```

### Динамический фильтр-билдер по всем атрибутам

```js
const attrs = await fetch('http://localhost:5050/api/attributes').then(r => r.json());

for (const a of attrs) {
  switch (a.valueType) {
    case 'enum': {
      // для каждого enum — dropdown со значениями из справочника
      const vals = await fetch(`http://localhost:5050/api/attributes/${a.id}/enum-values`).then(r => r.json());
      // рендер селектора
      break;
    }
    case 'numeric':  /* range slider / min-max inputs */ break;
    case 'bool':     /* checkbox */ break;
    case 'brand': {
      // brand — это отдельный справочник
      const brands = await fetch('http://localhost:5050/api/brands').then(r => r.json());
      break;
    }
    case 'text':     /* text input */ break;
  }
}
```

---

## 7. Ограничения (важно знать)

1. **Нет записи.** Только чтение. Добавление/изменение/удаление не поддерживается — БД наполняется из `seed.json` при старте.
2. **Нет аутентификации.** API открытый. В проде нужно добавить JWT / API-ключ.
3. **Нет CORS.** Для браузерных клиентов нужно включить CORS-policy на gateway.
4. **Нет rate-limiting.** Защиты от abuse нет.
5. **Поиск по `q` использует `ILIKE '%...%'`** — без индекса. На больших каталогах медленный; замена на `pg_trgm` / Postgres FTS в планах.
6. **Нет ETags / If-None-Match.** Кеширование на клиенте — только по TTL.
7. **Нет версионирования URL.** Пока `/api/*`; при изменении схемы будет `/api/v2/*`.
8. **Offset-based пагинация** — для товарных каталогов >100k рекомендуется keyset cursor (в планах).

---

## 8. Поднять локально

```bash
docker compose up -d --build
# postgres:  localhost:5433
# service:   localhost:5221
# gateway:   localhost:5050  ← клиентский порт

curl http://localhost:5050/health
curl http://localhost:5050/api/products/1
```

OpenAPI: `http://localhost:5050/openapi/v1.json`.

---

## 9. Что дальше

См. раздел «Варианты улучшений» в обсуждении (кеш справочников, pg_trgm, healthchecks, rate-limiting, JWT). В текущей версии все эти улучшения не применены — API находится в MVP/демо-состоянии.
