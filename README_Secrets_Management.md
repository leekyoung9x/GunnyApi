# Secrets Management Guide

## Vấn đề GitHub Push Protection

GitHub đã block push vì phát hiện secret keys trong `appsettings.json`. Đây là tính năng bảo mật để ngăn chặn việc vô tình push credentials lên public repository.

## Giải pháp đã áp dụng

### 1. Tách secrets ra khỏi appsettings.json

**appsettings.json** (commit được vào Git):
```json
{
  "PayMongoSettings": {
    "SecretKey": ""  // ← Để trống, không chứa secret
  }
}
```

**appsettings.Local.json** (KHÔNG commit vào Git):
```json
{
  "PayMongoSettings": {
    "SecretKey": "sk_test_kvz3JUti8M6PHQszRniXytYJ"  // ← Secret thật ở đây
  }
}
```

### 2. Thêm vào .gitignore

File `.gitignore` đã được cập nhật:
```
appsettings.Local.json
```

### 3. Cập nhật Program.cs

```csharp
// Load appsettings.Local.json if exists (for local secrets)
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
```

## Cách sử dụng

### Cho Developer mới

1. **Copy template file**:
   ```powershell
   Copy-Item appsettings.Local.json.template appsettings.Local.json
   ```

2. **Điền secrets thật vào `appsettings.Local.json`**:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=114.29.239.53,1433;Database=Db_Member;...",
       "TankConnection": "Server=114.29.239.53,1433;Database=Db_Tank41;..."
     },
     "JwtSettings": {
       "SecretKey": "YourSuperSecretKeyForJwtTokenGeneration123456789"
     },
     "GameSettings": {
       "LoginKey": "QY-16-WAN-0668-2555555-7ROAD-ditmemaydiloz-123456789"
     },
     "PayMongoSettings": {
       "SecretKey": "sk_test_kvz3JUti8M6PHQszRniXytYJ"
     }
   }
   ```

3. **File này sẽ KHÔNG được commit** vào Git (đã có trong .gitignore)

### Cho Production Server

**Option 1: Sử dụng Environment Variables**

```powershell
# Windows PowerShell
$env:PayMongoSettings__SecretKey = "sk_live_YOUR_PRODUCTION_KEY"
$env:JwtSettings__SecretKey = "YOUR_PRODUCTION_JWT_SECRET"
```

```bash
# Linux/Mac
export PayMongoSettings__SecretKey="sk_live_YOUR_PRODUCTION_KEY"
export JwtSettings__SecretKey="YOUR_PRODUCTION_JWT_SECRET"
```

**Option 2: Sử dụng appsettings.Production.json trên server**

Tạo file `appsettings.Production.json` trực tiếp trên server (không commit vào Git):
```json
{
  "PayMongoSettings": {
    "SecretKey": "sk_live_YOUR_PRODUCTION_KEY"
  }
}
```

**Option 3: Sử dụng Azure Key Vault / AWS Secrets Manager**

Cho môi trường enterprise.

## Configuration Precedence (Thứ tự ưu tiên)

ASP.NET Core load configuration theo thứ tự sau (sau override trước):

1. `appsettings.json` (base config)
2. `appsettings.{Environment}.json` (Development, Production, etc.)
3. `appsettings.Local.json` (local secrets) ← **Mới thêm**
4. Environment Variables (highest priority)
5. Command-line arguments

**Ví dụ**:
```json
// appsettings.json
"PayMongoSettings": { "SecretKey": "" }

// appsettings.Local.json (override)
"PayMongoSettings": { "SecretKey": "sk_test_abc123" }

// Environment Variable (override tất cả)
PayMongoSettings__SecretKey=sk_live_xyz789
```

Kết quả: `sk_live_xyz789` (từ Environment Variable)

## Giải quyết lỗi Git Push

### Bước 1: Xóa secret khỏi appsettings.json

✅ Đã hoàn thành - `SecretKey` đã được set thành `""`

### Bước 2: Amend commit để xóa secret khỏi history

```powershell
# Stage file đã sửa
git add GunnyApi/appsettings.json
git add .gitignore

# Amend commit hiện tại (thay thế commit có secret)
git commit --amend --no-edit

# Force push (vì đã thay đổi history)
git push origin feature/feat_new-login --force
```

**Lưu ý**: `--force` là an toàn vì đây là feature branch riêng của bạn.

### Bước 3: Nếu vẫn bị block, clean history hoàn toàn

Nếu secret đã tồn tại trong nhiều commits:

```powershell
# Option 1: Squash tất cả commits thành 1 commit mới
git reset --soft main  # hoặc branch gốc
git commit -m "feat: new login feature with CORS and Kestrel config"
git push origin feature/feat_new-login --force

# Option 2: Tạo branch mới từ code hiện tại
git checkout -b feature/feat_new-login-clean
git add .
git commit -m "feat: new login feature (secrets moved to Local config)"
git push origin feature/feat_new-login-clean
```

## Best Practices

### ✅ DO (Nên làm)

1. **Luôn dùng template files**:
   - `appsettings.Local.json.template` → commit vào Git
   - `appsettings.Local.json` → KHÔNG commit (trong .gitignore)

2. **Tách secrets theo môi trường**:
   ```
   appsettings.json                    ← Base config, no secrets
   appsettings.Local.json              ← Local development secrets
   appsettings.Production.json         ← Production secrets (on server only)
   ```

3. **Sử dụng Environment Variables cho production**:
   ```powershell
   $env:ConnectionStrings__DefaultConnection = "..."
   ```

4. **Document secrets trong README**:
   Liệt kê secrets nào cần thiết nhưng KHÔNG bao giờ ghi giá trị thật.

5. **Rotate secrets ngay nếu bị leak**:
   Nếu vô tình push secret lên Git, đổi secret mới ngay lập tức.

### ❌ DON'T (Không nên làm)

1. **KHÔNG hardcode secrets trong code**:
   ```csharp
   // ❌ WRONG
   var apiKey = "sk_test_abc123";
   ```

2. **KHÔNG commit files chứa secrets**:
   - ❌ `appsettings.Development.json`
   - ❌ `appsettings.Local.json`
   - ❌ `.env` files

3. **KHÔNG dùng secrets thật trong code example**:
   ```json
   // ❌ WRONG
   "SecretKey": "sk_test_kvz3JUti8M6PHQszRniXytYJ"
   
   // ✅ CORRECT
   "SecretKey": "YOUR_SECRET_KEY_HERE"
   ```

4. **KHÔNG share secrets qua chat/email**:
   Dùng password managers hoặc secure sharing tools.

## Kiểm tra secrets trước khi commit

### Option 1: Manual check
```powershell
# Xem những gì sẽ được commit
git diff --cached
```

### Option 2: Git hooks (pre-commit)
Tạo file `.git/hooks/pre-commit`:
```bash
#!/bin/sh
if git diff --cached | grep -E "(sk_test_|sk_live_|password|secret)"; then
    echo "⚠️  WARNING: Possible secret detected!"
    exit 1
fi
```

### Option 3: Tools
- [git-secrets](https://github.com/awslabs/git-secrets)
- [gitleaks](https://github.com/gitleaks/gitleaks)
- [truffleHog](https://github.com/trufflesecurity/trufflehog)

## Troubleshooting

### Q: Ứng dụng không load được secrets từ appsettings.Local.json

**A**: Kiểm tra:
1. File có tồn tại không: `Test-Path appsettings.Local.json`
2. JSON syntax có đúng không (dùng JSON validator)
3. `Program.cs` có load file này không
4. Build mode có đúng không (Development/Production)

### Q: GitHub vẫn block push sau khi xóa secret

**A**: Secret vẫn tồn tại trong Git history. Cần:
1. Amend commit: `git commit --amend`
2. Hoặc tạo branch mới clean
3. Hoặc rewrite history: `git filter-branch` (advanced)

### Q: Làm sao share secrets với team?

**A**: Các options:
1. **Password Manager** (1Password, LastPass, Bitwarden)
2. **Secret Management Service** (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault)
3. **Encrypted messaging** (Signal, Wire)
4. **KHÔNG qua**: Email, Slack, SMS, Git

## Checklist trước khi commit

- [ ] `appsettings.json` không chứa secrets (hoặc là giá trị placeholder)
- [ ] `appsettings.Local.json` đã trong .gitignore
- [ ] Template file (`appsettings.Local.json.template`) đã được tạo
- [ ] README đã hướng dẫn cách setup secrets
- [ ] Đã test với secrets từ Local config (không phải hardcode)
- [ ] `git diff --cached` không hiển thị secret nào

## Next Steps

Sau khi fix xong, push code:

```powershell
# Add files
git add GunnyApi/appsettings.json
git add GunnyApi/appsettings.Local.json.template
git add .gitignore
git add GunnyApi/Program.cs

# Commit (amend để replace commit có secret)
git commit --amend -m "feat: new login feature with proper secrets management"

# Force push (vì đã amend)
git push origin feature/feat_new-login --force
```

✅ Bây giờ push sẽ thành công!
