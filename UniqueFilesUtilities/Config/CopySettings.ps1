# Copies all of the settings files from the solution's Config folder to the live settings location:
#	C:\Users\username\OneDrive\Files and Storage\AppConfig

Copy-Item -Force -Path "Config\*.txt" -Destination "C:\Users\ezoch\OneDrive\Files and Storage\AppConfig"