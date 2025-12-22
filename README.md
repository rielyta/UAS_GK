# Game Pesawat

Sebuah game 3D berbasis Unity yang menampilkan permainan pesawat dengan berbagai mekanik gameplay.

##  Deskripsi Tugas

Proyek ini adalah implementasi dari tugas pemrograman grafika komputer yang mengharuskan pembuatan aplikasi Unity untuk mendemonstrasikan konsep-konsep dasar grafis komputer, termasuk:

- Penggunaan objek 3D dan prefab
- Transformasi geometri (translasi, rotasi, dan skala)
- Interaksi pengguna melalui input keyboard dan mouse
- Pembuatan shader kustom dengan efek visual dinamis

##  Pemenuhan Ketentuan Tugas

### 1. **Jenis Objek (Prefab)**
Proyek memiliki **3 jenis objek berbeda** dengan karakteristik unik:
- **Pesawat**: Objek utama yang dapat dikontrol oleh pemain (1 instance)
- **Peluru**: Objek yang di-spawn sesuai aksi pemain (penembakan) - dibuat dengan object pooling
- **Musuh**: Objek yang di-spawn secara prosedural dan otomatis dalam jumlah banyak melalui EnemySpawner

### 2. **Transformasi Geometri**
Proyek menerapkan **3 jenis transformasi** yang diimplementasikan secara manual melalui script:

| Objek | Jenis Transformasi | Implementasi |
|-------|-------------------|--------------|
| **Pesawat** | Translasi + Rotasi | Dikendalikan pengguna via keyboard/mouse dalam `Pesawat.cs` |
| **Peluru** | Translasi | Gerakan forward otomatis dalam `Bullet.cs` |
| **Musuh** | Rotasi + Skala | Pola gerakan dan animasi dalam `Enemy.cs` |

**Catatan Penting**: Semua transformasi diimplementasikan **tanpa menggunakan fungsi built-in otomatis** seperti `MoveTo` atau `Translate animator`. Setiap gerakan dilakukan dengan manipulasi langsung nilai `transform.position`, `transform.rotation`, dan `transform.scale`.

### 3. **Shader Kustom**
Proyek menggunakan **shader kustom yang dibuat sendiri** (bukan shader default Unity):

| Shader | Objek | Fitur Khusus |
|--------|-------|-------------|
| **PesawatShader.shader** | Pesawat | Memiliki animasi yang dapat di-trigger: efek glow saat menembak (mengubah intensitas emissive) |
| **Enemy Shader** | Musuh | Shader unik dengan parameter warna dinamis |
| **Bullet Shader** | Peluru | Shader sederhana dengan efek visual berbeda |


---

##  Fitur Utama dan Mekanik Permainan

- **Sistem Kontrol Pesawat**: Pemain dapat mengendalikan pesawat dengan keyboard dan mouse
  - Gerakan ke depan, belakang, kiri, dan kanan (translasi manual)
  - Gerakan ke atas dan ke bawah (translasi vertikal)
  - Rotasi pada tiga sumbu: roll, pitch, yaw (rotasi manual)

- **Sistem Penembakan**: Pesawat dapat menembakkan peluru kepada musuh
  - Penembakan dengan interval waktu tertentu (cooldown)
  - Pola penyebaran peluru ganda
  - Efek suara saat menembak
  - Optimasi performa dengan object pooling

- **Sistem Musuh**: Musuh muncul secara otomatis selama permainan berlangsung
  - Sistem spawn prosedural otomatis untuk musuh
  - Gerakan musuh mengikuti pola tertentu (rotasi dan translasi)
  - Sistem kesehatan untuk musuh
  - Visual yang dapat disesuaikan

- **Shader Kustom**: Efek visual khusus untuk setiap jenis objek
  - Shader dengan dukungan Universal Render Pipeline
  - Efek cahaya dinamis saat pesawat menembak
  - Animasi shader yang dapat di-trigger melalui script
  - Dukungan berbagai tekstur dan warna

- **Antarmuka Pengguna**: Menampilkan informasi permainan
  - Pengelola antarmuka untuk menu dan HUD
  - Tampilan kesehatan pemain
  - Sistem poin permainan

- **Fisika dan Tabrakan**: Sistem fisika untuk gerakan dan interaksi
  - Gerakan berbasis Rigidbody
  - Deteksi dan respons tabrakan
  - Efek benturan (knockback)

## 📁 Struktur Folder

```
Assets/
├── Script/                      # Kode program (C#)
│   ├── Pesawat.cs             # Kontrol pesawat utama
│   ├── Bullet.cs              # Mekanik peluru
│   ├── BulletPool.cs          # Manajemen pool peluru
│   ├── Enemy.cs               # Perilaku musuh
│   ├── EnemySpawner.cs        # Sistem pemunculan musuh
│   ├── UIManager.cs           # Pengelola antarmuka
│   └── ...
├── Shaders/                    # Shader kustom
│   └── PesawatShader.shader
├── Scenes/
│   └── SampleScene.unity       # Scene utama permainan
├── Materials/                  # Material aset
├── Prefab/                     # Prefab yang dapat digunakan kembali
├── Resources/                  # Sumber daya runtime
└── Settings/                   # Pengaturan rendering
```

##  Detail Implementasi Transformasi Manual

Semua transformasi diimplementasikan secara **manual tanpa menggunakan fungsi built-in otomatis**:

### Pesawat (Translasi + Rotasi - Dikendalikan Pengguna)
```csharp
// Dalam Pesawat.cs
transform.position += direction * kecepatan * Time.deltaTime;  // Translasi manual
transform.rotation *= Quaternion.Euler(pitchInput, yawInput, rollInput);  // Rotasi manual
```

### Peluru (Translasi - Otomatis)
```csharp
// Dalam Bullet.cs
transform.position += transform.forward * bulletSpeed * Time.deltaTime;  // Translasi maju
```

### Musuh (Rotasi + Skala - Otomatis)
```csharp
// Dalam Enemy.cs
transform.Rotate(rotationSpeed * Time.deltaTime);  // Rotasi manual
// Animasi skala untuk efek visual yang dinamis
```
