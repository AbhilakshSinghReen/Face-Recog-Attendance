import urllib.request
import cv2
import numpy as np


cap = cv2.VideoCapture("rtsp://admin:Password!@192.168.1.101/H.264/media.smp")
while True:

    _,frame = cap.read();
    # all the opencv processing is done here
    #cv2.imshow(cap)
    frame = cv2.resize(frame,None,fx=0.5,fy=0.5)
    cv2.imshow("Frame",frame)
    if ord('q')==cv2.waitKey(10):
        exit(0)
