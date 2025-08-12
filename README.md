# Chat Uygulaması

## Projenin Amacı

Bu uygulama localhost'ta farklı makineler arasında iletişim için kurulmuştur. Sanal makineler arasında Wireshark ile dinleme etkinlikleri yapılmıştır. İlk önce local ile sonra da SSL ile şifreli metinler gösterilmiştir. Daha sonra MITM (Man-in-the-Middle) saldırısı için 3 farklı sanal makine ile eş zamanlı çalışmalar yapılmıştır.

Bu proje, ağ güvenliği ve kriptografi konularında pratik deneyim kazanmak, farklı güvenlik protokollerinin nasıl çalıştığını anlamak ve güvenlik açıklarını tespit etmek amacıyla geliştirilmiştir.

---

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

## Güvenlik Testleri ve Ağ Analizi

### Wireshark ile Ağ Trafiği Dinleme

Bu proje, ağ güvenliği konularında pratik deneyim kazanmak amacıyla geliştirilmiştir. Aşağıdaki güvenlik testleri gerçekleştirilmiştir:

#### 1. Localhost İletişim Analizi
- Farklı sanal makineler arasında localhost üzerinden iletişim
- Wireshark ile HTTP trafiğinin dinlenmesi
- Açık metin mesajların yakalanması

#### 2. SSL/TLS Şifreleme Testleri
- HTTPS protokolü ile güvenli iletişim
- Şifreli trafiğin Wireshark ile analizi
- Sertifika doğrulama süreçleri

#### 3. MITM (Man-in-the-Middle) Saldırı Simülasyonu
- 3 farklı sanal makine ile eş zamanlı çalışma
- Ağ trafiğinin dinlenmesi ve manipülasyonu
- Güvenlik açıklarının tespit edilmesi
- **Saldırgan Makineler:** Kali Linux, Ubuntu ve macOS kullanılarak farklı işletim sistemlerinde saldırı vektörleri test edilmiştir

### Güvenlik Test Ortamı

- **Sanal Makine 1:** MetChat Sunucu (.NET Core)
- **Sanal Makine 2:** MetChat İstemci (Web Browser)
- **Saldırgan Makineler:** 
  - **Kali Linux:** Penetrasyon testi araçları ve ağ analizi
  - **Ubuntu:** Açık kaynak güvenlik araçları
  - **macOS:** Cross-platform saldırı vektörleri
- **Araçlar:** Wireshark, SSL/TLS test araçları, ağ protokol analizörleri

### Test Senaryoları

1. **Açık Metin Testi:** 
   - HTTP üzerinden gönderilen mesajların yakalanması
   - Wireshark ile plain text mesajların görüntülenmesi
   - Ağ trafiğinde hassas bilgilerin açık şekilde aktarılması

2. **Şifreli İletişim Testi:** 
   - HTTPS ile korunan mesajların güvenliği
   - Wireshark'da şifreli trafiğin analizi
   - SSL/TLS protokolünün güvenlik katmanları

3. **MITM Saldırı Testi:** 
   - Ağ trafiğinin manipülasyonu ve güvenlik önlemleri
   - Farklı işletim sistemlerinde saldırı vektörlerinin test edilmesi
   - Cross-platform güvenlik açıklarının tespiti

Bu testler sayesinde, modern web uygulamalarında güvenlik protokollerinin önemi ve uygulanması gereken güvenlik önlemleri pratik olarak öğrenilmiştir. Wireshark analizleri ile mesajların hem şifreli hem de şifresiz hallerinin nasıl görüntülendiği gözlemlenmiş, HTTP ve HTTPS protokollerinin arasındaki farklar net bir şekilde anlaşılmıştır. Farklı işletim sistemlerinde (Kali Linux, Ubuntu, macOS) gerçekleştirilen saldırı simülasyonları sayesinde, iletişim protokollerinin nasıl çalıştığı ve güvenlik açıklarının nasıl tespit edilebileceği konusunda kapsamlı bilgi sahibi olunmuştur.

---

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