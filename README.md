# Game Pesawat

Sebuah game 3D berbasis Unity yang menampilkan permainan pesawat dengan berbagai mekanik gameplay.

## 📚 Deskripsi Tugas

Proyek ini adalah implementasi dari tugas pemrograman komputer grafis yang mengharuskan pembuatan aplikasi Unity untuk mendemonstrasikan konsep-konsep dasar grafis komputer, termasuk:

- Penggunaan objek 3D dan prefab
- Transformasi geometri (translasi, rotasi, dan skala)
- Interaksi pengguna melalui input keyboard dan mouse
- Pembuatan shader kustom dengan efek visual dinamis

## ✅ Pemenuhan Ketentuan Tugas

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

Salah satu shader (PesawatShader) memiliki **animasi yang dapat di-trigger melalui script** menggunakan `PesawatShaderAnimation.cs` yang mengubah intensitas emissive saat pesawat menembak.

---

## 🎮 Fitur Utama dan Mekanik Permainan

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

## 🎯 Detail Implementasi Transformasi Manual

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

---

## 🎯 Komponen Utama Kode

### Pesawat.cs
Kode untuk mengontrol pesawat pemain dengan transformasi yang dikendalikan pengguna:
- Menerima input dari keyboard (WASD) dan mouse
- Mengelola gerakan translasi (perpindahan posisi) pesawat
- Mengelola rotasi pesawat pada tiga sumbu (roll, pitch, yaw)
- Sistem kesehatan pesawat (maksimal 3 poin)
- Mekanisme penembakan dengan trigger pengguna
- Penanganan tabrakan

### Bullet.cs
Kode untuk mekanisme peluru dengan transformasi translasi otomatis:
- Gerakan maju otomatis (translasi ke depan)
- Perhitungan kerusakan peluru
- Pengecekan batas area permainan
- Variasi warna peluru

### Enemy.cs dan EnemySpawner.cs
Kode untuk sistem musuh dengan transformasi rotasi dan skala:
- Pemunculan musuh secara prosedural dan otomatis
- Pola gerakan musuh dengan rotasi manual
- Efek visual dengan perubahan skala
- Sistem kesehatan untuk musuh

### PesawatShaderAnimation.cs
Kode untuk animasi shader yang dapat di-trigger melalui aksi pengguna:
- Mengubah intensitas emissive saat pesawat menembak
- Animasi properti material secara dinamis
- Trigger melalui event penembakan

## ⌨️ Kontrol Permainan

| Tombol | Fungsi |
|--------|--------|
| **W/A/S/D** | Gerak pesawat (maju/kiri/mundur/kanan) |
| **Space** | Gerak ke atas |
| **Ctrl** | Gerak ke bawah |
| **Gerakan Mouse** | Ubah arah pandang pesawat |
| **Q/E** | Rotasi pesawat kiri/kanan |
| **Klik Kiri Mouse** | Tembak |

## 🔧 Persyaratan Sistem

- **Versi Unity**: 2022.x atau lebih baru
- **Pipeline Rendering**: Universal Render Pipeline (URP)
- **Platform**: Windows, macOS, atau Linux
- **Versi C#**: 9.0 atau lebih tinggi

## 🚀 Cara Menjalankan Permainan

1. Buka proyek ini di Unity Editor
2. Pastikan Universal Render Pipeline sudah terpasang
3. Buka scene `Assets/Scenes/SampleScene.unity`
4. Tekan tombol **Play** untuk memulai permainan
5. Gunakan kontrol yang telah dijelaskan di atas untuk bermain

## 📊 Spesifikasi Pesawat

- **Kesehatan Maksimal**: 3 poin
- **Kecepatan**: 10 satuan/detik
- **Kecepatan Roll**: 180°/detik
- **Kecepatan Pitch**: 180°/detik
- **Kecepatan Yaw**: 150°/detik
- **Sensitivitas Mouse**: 5,5x
- **Kecepatan Peluru**: 20 satuan/detik
- **Waktu Tunggu Penembakan**: 0,2 detik
- **Penyebaran Peluru**: 10°

## 🎨 Grafis dan Rendering

Proyek ini menggunakan:
- **Universal Render Pipeline (URP)**: Untuk sistem rendering modern
- **Shader Kustom**: Untuk efek visual khusus pada pesawat
- **Warna Emisi (HDR)**: Untuk efek cahaya yang menarik saat pesawat menembak
- **Properti Material Dinamis**: Untuk animasi efek visual shader

## 🐛 Petunjuk Debugging

Untuk membantu mengatasi masalah:
- Periksa konsol Unity untuk pesan kesalahan
- Gunakan Scene view untuk melihat posisi dan area tabrakan
- Gunakan Gizmos untuk memvisualisasikan pola gerakan

## 📝 Catatan Khusus

- Proyek menggunakan Input System baru dari Unity
- Gaya gravitasi dinonaktifkan untuk memberikan kontrol penuh terhadap gerakan pesawat
- Efek benturan diterapkan untuk meningkatkan pengalaman bermain
- Object pooling digunakan untuk mengoptimalkan performa penembakan peluru

## 📜 Lisensi

Proyek ini dikembangkan sebagai bagian dari tugas akademis.

---

**Pembaruan Terakhir**: Desember 2025
