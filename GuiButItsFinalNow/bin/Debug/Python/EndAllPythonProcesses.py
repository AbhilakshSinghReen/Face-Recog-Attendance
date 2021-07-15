import os

try:
    os.system('TASKKILL /F /IM python.exe')
except:
    print("Failed")
