# Сборка всего Solution
$devenv = "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.com"
& $devenv CloudBackuper.sln /Build "Release"

# Сборка фронтенда
Set-Location -Path Frontend_v2
& npm run build
Set-Location -Path ..\

# Пересоздаём основную директорию
Remove-Item -ErrorAction Ignore -Recurse -Force DIST
New-Item -ItemType Directory DIST

# Копируем основной проект и службу 
Copy-item -Force -Recurse -Verbose .\CloudBackuper\bin\Release\* .\DIST\
Copy-item -Force -Recurse -Verbose .\ServiceRunner\bin\Release\* .\DIST\

# Удаляем лишние каталоги
Remove-Item -ErrorAction Ignore -Recurse -Force DIST\plugins
Remove-Item -ErrorAction Ignore -Recurse -Force DIST\utils
Remove-Item -ErrorAction Ignore -Recurse -Force DIST\WebApp
Remove-Item -ErrorAction Ignore -Recurse -Force DIST\WebApp

# Создаём основные каталоги
New-Item -ItemType Directory DIST\plugins
New-Item -ItemType Directory DIST\utils
New-Item -ItemType Directory DIST\WebApp

# Копируем плагины и утилиты
Copy-item -Force -Recurse -Verbose .\Plugin_AmazonS3\bin\Release\ .\DIST\plugins\amazon_s3
Copy-item -Force -Recurse -Verbose .\Plugin_YandexDisk\bin\Release\ .\DIST\plugins\yandex_disk
Copy-item -Force -Recurse -Verbose .\WinClient\bin\Release\* .\DIST\utils
Copy-item -Force -Recurse -Verbose .\MaskPreview\bin\Release\* .\DIST\utils

# Копируем фронтенд
Copy-item -Force -Recurse -Verbose .\Frontend_v2\dist\* .\DIST\WebApp

# Чистим мусор вроде XML файлов документации
Remove-Item -ErrorAction Ignore -Recurse -Include *.xml DIST
Remove-Item -ErrorAction Ignore -Recurse -Include *.lnk DIST
Remove-Item -ErrorAction Ignore -Recurse -Include *.log DIST
Remove-Item -ErrorAction Ignore -Recurse -Include config.debug.json DIST
Remove-Item -ErrorAction Ignore -Recurse -Include *cleanup_dir.bat DIST
Remove-Item -ErrorAction Ignore -Recurse -Include *cleanup.bat DIST
Remove-Item -ErrorAction Ignore -Recurse -Include Release.zip DIST
Remove-Item -ErrorAction Ignore -Recurse -Include Debug.zip DIST

# Удаляем конфиги с приватными данными
Remove-Item -ErrorAction Ignore -Recurse -Include config.json DIST 
Remove-Item -ErrorAction Ignore -Recurse -Include config.yml DIST 
# Восстанавливаем конфиг
Copy-item .\DIST\config_example.yml .\DIST\config.yml 