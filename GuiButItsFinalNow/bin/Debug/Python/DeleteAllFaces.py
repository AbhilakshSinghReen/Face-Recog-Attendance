
"""
import ctypes
import os
import win32process

hwnd = ctypes.windll.kernel32.GetConsoleWindow()
if hwnd != 0:
    ctypes.windll.user32.ShowWindow(hwnd, 0)
    ctypes.windll.kernel32.CloseHandle(hwnd)
    _, pid = win32process.GetWindowThreadProcessId(hwnd)
    os.system('taskkill /PID ' + str(pid) + ' /f')
"""
import os
import os.path
import time
import cv2
import sys
#import path
import glob

dirname = os.path.dirname(__file__)

fh=open(os.path.join(dirname,'Config/ImageDeleteTimePeriod.txt'),'r')
NumberOfHours=int(fh.readline())
fh.close()


img_dir = "Save2Images" # Enter Directory of all images

import os, time, sys

fh=open(os.path.join(dirname,'Config/AllFacesSaveLocation.txt'),'r')
AllFacesSaveLocation=str(fh.readline())
fh.close

AllFacesSaveLocation= AllFacesSaveLocation.replace("\n","")
AllFacesSaveLocation.replace(os.sep,"/")


folder_path = AllFacesSaveLocation
file_ends_with = ".jpg"
how_many_days_old_logs_to_remove = NumberOfHours/24

now = time.time()
only_files = []

for file in os.listdir(folder_path):
    file_full_path = os.path.join(folder_path,file)
    if os.path.isfile(file_full_path) and file.endswith(file_ends_with):
        #Delete files older than x days
        if os.stat(file_full_path).st_ctime < now - how_many_days_old_logs_to_remove * 86400:
             os.remove(file_full_path)
             print("/n File Removed : ")# , file_full_path


while True:
    #print("NExt Loop")
    now = time.time()
    only_files = []

    for file in os.listdir(folder_path):

        file_full_path = os.path.join(folder_path,file)
        if os.path.isfile(file_full_path) and file.endswith(file_ends_with):

            #Delete files older than x days
            if os.stat(file_full_path).st_mtime < now - how_many_days_old_logs_to_remove * 86400:
                print(str(file))
                os.remove(file_full_path)
                print("/n File Removed : ")# , file_full_path
