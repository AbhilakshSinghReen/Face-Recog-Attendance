import urllib.request
import cv2
import numpy as np
import os
import glob
import time
#cap = cv2.VideoCapture("rtsp://admin:Password!1@192.168.1.101/MediaInput/h265/string_2")


img_dir = "C:\\Users\\Lavi\\Pictures\\Wallpapers" # Enter Directory of all images
data_path = os.path.join(img_dir,'*g')
files = glob.glob(data_path)
data = []
for f1 in files:
    img = cv2.imread(f1)
    cv2.imshow("Frame",img)
    time.sleep(1)
    if ord('q')==cv2.waitKey(10):
        exit(0)
