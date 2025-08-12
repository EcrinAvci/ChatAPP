# Chat Uygulaması

.NET Core ile Razor Pages kullanılarak geliştirilmiş web tabanlı mesajlaşma uygulaması.

## Özellikler

- Real-time mesajlaşma
- SQLite veritabanı
- RESTful API endpoints
- Modern ve responsive UI
- Postman ile API test desteği

## Teknolojiler

- .NET Core 9.0
- ASP.NET Core Razor Pages
- Entity Framework Core
- SQLite
- Bootstrap 5
- JavaScript (Fetch API)

## Kurulum

1. Projeyi klonlayın
2. Gerekli paketleri yükleyin:
   ```bash
   dotnet restore
   ```
3. Veritabanını oluşturun:
   ```bash
   dotnet ef database update
   ```
4. Uygulamayı çalıştırın:
   ```bash
   dotnet run
   ```

## API Endpoints

### GET /api/ChatApi
Tüm mesajları getirir (son 50 mesaj)

**Response:**
```json
[
  {
    "id": 1,
    "content": "Merhaba!",
    "senderName": "Ahmet",
    "timestamp": "2025-08-06T10:30:00",
    "isRead": false
  }
]
```

### POST /api/ChatApi
Yeni mesaj gönderir

**Request Body:**
```json
{
  "senderName": "Ahmet",
  "content": "Merhaba!"
}
```

**Response:**
```json
{
  "id": 1,
  "content": "Merhaba!",
  "senderName": "Ahmet",
  "timestamp": "2025-08-06T10:30:00",
  "isRead": false
}
```

### GET /api/ChatApi/{id}
Belirli bir mesajı getirir

### PUT /api/ChatApi/{id}
Mesajı günceller

### DELETE /api/ChatApi/{id}
Mesajı siler

## Postman Test Koleksiyonu

### 1. Mesaj Gönderme
- **Method:** POST
- **URL:** `http://localhost:5000/api/ChatApi`
- **Headers:** `Content-Type: application/json`
- **Body:**
```json
{
  "senderName": "TestUser",
  "content": "Bu bir test mesajıdır!"
}
```

### 2. Mesajları Listeleme
- **Method:** GET
- **URL:** `http://localhost:5000/api/ChatApi`

### 3. Belirli Mesajı Getirme
- **Method:** GET
- **URL:** `http://localhost:5000/api/ChatApi/1`

### 4. Mesaj Güncelleme
- **Method:** PUT
- **URL:** `http://localhost:5000/api/ChatApi/1`
- **Headers:** `Content-Type: application/json`
- **Body:**
```json
{
  "id": 1,
  "senderName": "TestUser",
  "content": "Güncellenmiş mesaj!",
  "timestamp": "2025-08-06T10:30:00",
  "isRead": true
}
```

### 5. Mesaj Silme
- **Method:** DELETE
- **URL:** `http://localhost:5000/api/ChatApi/1`

## Kullanım

1. Uygulamayı çalıştırdıktan sonra `http://localhost:5000` adresine gidin
2. Ana sayfada "Chat'e Başla" butonuna tıklayın
3. Adınızı girin ve mesajlarınızı yazın
4. Enter tuşu veya "Gönder" butonu ile mesaj gönderin
5. Mesajlar otomatik olarak 5 saniyede bir güncellenir

## Veritabanı

SQLite veritabanı `chat.db` dosyasında saklanır. Veritabanı şeması:

```sql
CREATE TABLE Messages (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Content TEXT NOT NULL,
    SenderName TEXT NOT NULL,
    Timestamp TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    IsRead INTEGER NOT NULL
);
```

## Geliştirme

### Yeni Migration Ekleme
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Veritabanını Sıfırlama
```bash
dotnet ef database drop
dotnet ef database update
``` 