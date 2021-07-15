'''
Main program
@Author: David Vu

To execute simply run:
main.py

To input new user:
main.py --mode "input"

'''
from imutils.video import FileVideoStream
import cv2
from align_custom import AlignCustom
from face_feature import FaceFeature
from mtcnn_detect import MTCNNDetect
from tf_graph import FaceRecGraph
import argparse
import sys
import json
import time
import numpy as np
import numpy as np
import os
import glob
#rom PIL import Image
import urllib.request


#url='http://192.168.1.101/cgi-bin/camera'

#####http://admin:Password!1@192.168.1.101/cgi-bin/camera

TIMEOUT = 10 #10 seconds

def main(args):
    mode = args.mode
    if(mode == "camera"):
        camera_recog()
    elif mode == "input":
        create_manual_data();
    else:
        raise ValueError("Unimplemented mode")
'''
Description:
Images from Video Capture -> detect faces' regions -> crop those faces and align them
    -> each cropped face is categorized in 3 types: Center, Left, Right
    -> Extract 128D vectors( face features)
    -> Search for matching subjects in the dataset based on the types of face positions.
    -> The preexisitng face 128D vector with the shortest distance to the 128D vector of the face on screen is most likely a match
    (Distance threshold is 0.6, percentage threshold is 70%)

'''
"""
def camera_recog():
    print("[INFO] camera sensor warming up...")
#    vs = cv2.VideoCapture(0); #get input from webcam

    #vs = cv2.VideoCapture("C:\\Users\\Lavi\\Desktop\\vid1.mp4"); #get input from webcam

    #vs = cv2.VideoCapture("rtsp://admin:Password!@192.168.1.101/H.264/media.smp")
    #fvs = FileVideoStream("rtsp://admin:Password!@192.168.1.101/H.264/media.smp").start()
    alt = 0
    detect_time = time.time()

    img_dir = "C:/Users/Lavi/Desktop/faced/Images" # Enter Directory of all images

#    img_dir = "C:/Users/Lavi/Desktop/NewFlask (1)/Images" # Enter Directory of all images
    copy_dir = "C:/Users/Lavi/Desktop/NewFlask (1)/copied"

    data_path = os.path.join(img_dir,'*g')
    files = glob.glob(data_path)
    data = []
    while True:
        files = glob.glob(data_path)
        #data = []
        for f1 in files:
        #    imgResp1=urllib.request.urlopen(url)
        #    imgNp1=np.array(bytearray(imgResp1.read()),dtype=np.uint8)
        #    img1=cv2.imdecode(imgNp1,-1)
            #_,frame = vs.read();
            #
            #frame = fvs.read()
            #frame = img1
            #imagetemp = Image.open(f1)
            #os.chdir("C:\\Users\\Lavi\\Desktop\\[y,best]FaceRec-master (1)\\FaceRec-master")
            frame = cv2.imread(f1)
            try:
                #u can certainly add a roi here but for the sake of a demo i'll just leave it as simple as this
                rects, landmarks = face_detect.detect_face(frame,20);#min face size is set to 80x80, distance, larger better
                aligns = []
                positions = []
                if alt<2 :
                    for (i, rect) in enumerate(rects):
                        aligned_face, face_pos = aligner.align(160,frame,landmarks[:,i])
                        if len(aligned_face) == 160 and len(aligned_face[0]) == 160:
                            aligns.append(aligned_face)
                            positions.append(face_pos)
                        else:
                            print("Align face failed") #log
                    if(len(aligns) > 0):
                        features_arr = extract_feature.get_features(aligns)
                        recog_data = findPeople(features_arr,positions)
                        for (i,rect) in enumerate(rects):
                        #cv2.rectangle(frame,(rect[0],rect[1]),(rect[2],rect[3]),(255,0,0)) #draw bounding box for the face
                            cv2.putText(frame,recog_data[i][0]+""+str(recog_data[i][1])+"",(rect[0],rect[1]),cv2.FONT_HERSHEY_SIMPLEX,1,(255,255,0),1,cv2.LINE_AA)
                            print(str(recog_data[i][0]))
                    #alt+=3
                #alt-=1

                #frame = cv2.resize(frame,None,fx=0.5,fy=0.5)
                cv2.imshow("Frame",frame)

                try:
                    os.remove(f1)
                #    print("True")
                except:
                    print("Delete excption"+f1)
                #imagetemp.save("C:\\Users\\Lavi\\Downloads\\Face-Track-Detect-Extract-master\\Face-Track-Detect-Extract-master\\facepics\\test")
                #if img_dir in f1:
                #    c2 = f1.replace(img_dir,'C:\\Users\\Lavi\\Downloads\\Face-Track-Detect-Extract-master\\Face-Track-Detect-Extract-master\\facepics\\test')
                #    print("True")

            #    os.chdir("C:\\Users\\Lavi\\Downloads\\Face-Track-Detect-Extract-master\\Face-Track-Detect-Extract-master\\facepics\\test")
                #cv2.imwrite(c2,frame)
            #    print(f1)
            #    time.sleep(1)
                key = cv2.waitKey(1) & 0xFF
                if key == ord("q"):
                    break
            except:
                print("No photo")

        key = cv2.waitKey(1) & 0xFF
        if key == ord("q"):
            break
"""
'''
facerec_128D.txt Data Structure:
{
"Person ID": {
    "Center": [[128D vector]],
    "Left": [[128D vector]],
    "Right": [[128D Vector]]
    }
}
This function basically does a simple linear search for
^the 128D vector with the min distance to the 128D vector of the face on screen
'''
def findPeople(features_arr, positions, thres = 0.5, percent_thres = 70):
    '''
    :param features_arr: a list of 128d Features of all faces on screen
    :param positions: a list of face position types of all faces on screen
    :param thres: distance threshold
    :return: person name and percentage
    '''
    f = open('./facerec_128D.txt','r')
    data_set = json.loads(f.read());
    returnRes = [];
    for (i,features_128D) in enumerate(features_arr):
        result = "";
        smallest = sys.maxsize
        for person in data_set.keys():
            person_data = data_set[person][positions[i]];
            for data in person_data:
                distance = np.sqrt(np.sum(np.square(data-features_128D)))
                if(distance < smallest):
                    smallest = distance;
                    result = person;
        percentage =  min(100, 100 * thres / smallest)
        if percentage <= percent_thres :
            result = ""
            print(str(result))
        returnRes.append((result,""))
        #returnRes = result
    return returnRes

'''
Description:
User input his/her name or ID -> Images from Video Capture -> detect the face -> crop the face and align it
    -> face is then categorized in 3 types: Center, Left, Right
    -> Extract 128D vectors( face features)
    -> Append each newly extracted face 128D vector to its corresponding position type (Center, Left, Right)
    -> Press Q to stop capturing
    -> Find the center ( the mean) of those 128D vectors in each category. ( np.mean(...) )
    -> Save

'''
dirname = os.path.dirname(__file__)

fh=open(os.path.join(dirname,'NewEmployee/Details.txt'),'r')
Id_this=fh.readline()
fh.close()
Id_this=Id_this.replace("\n","")

fh=open(os.path.join(dirname,'NewEmployee/Params.txt'),'r')
Cc=fh.readline()
Cc=Cc.replace("\n","")
IsWebcam=False
for i in range (0,5):
    try:
        if((i)==int(Cc)):
            IsWebcam=True
    except: pass
if(IsWebcam):
    vs=cv2.VideoCapture(int(Cc))
else:
    vs=cv2.VideoCapture(str(Cc))


fh.close()


def create_manual_data():
    global vs
    #vs = cv2.VideoCapture(vvar); #get input from webcam
    #imgResp1=urllib.request.urlopen(url)
#    imgNp1=np.array(bytearray(imgResp1.read()),dtype=np.uint8)
#    img1=cv2.imdecode(imgNp1,-1)
    print("Please input new user ID:")
    print(Id_this)
    new_name = Id_this #ez python input()
    f = open('./facerec_128D.txt','r');
    print("Database Open")
    data_set = json.loads(f.read());
    person_imgs = {"Left" : [], "Right": [], "Center": []};
    person_features = {"Left" : [], "Right": [], "Center": []};
    print("Please start turning slowly. Press 'q' to save and add this new user to the dataset");
    while True:
        _, frame = vs.read();
        #imgResp1=urllib.request.urlopen(url)
    #    imgNp1=np.array(bytearray(imgResp1.read()),dtype=np.uint8)
    #    img1=cv2.imdecode(imgNp1,-1)
    #    frame = img1
        rects, landmarks = face_detect.detect_face(frame, 80);  # min face size is set to 80x80
        for (i, rect) in enumerate(rects):
            aligned_frame, pos = aligner.align(160,frame,landmarks[:,i]);
            if len(aligned_frame) == 160 and len(aligned_frame[0]) == 160:
                person_imgs[pos].append(aligned_frame)
                cv2.imshow("Captured face", aligned_frame)
        key = cv2.waitKey(1) & 0xFF
        if key == ord("q"):
            break

    for pos in person_imgs: #there r some exceptions here, but I'll just leave it as this to keep it simple
        person_features[pos] = [np.mean(extract_feature.get_features(person_imgs[pos]),axis=0).tolist()]
    data_set[new_name] = person_features;
    f = open('./facerec_128D.txt', 'w');
    f.write(json.dumps(data_set))
    #dirname = os.path.dirname(__file__)

    NewFile=open(os.path.join(dirname,'NewEmployee/TrainingConfirmation.txt'),'w')
    NewFile.write("True")
    NewFile.close()





if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", type=str, help="Run camera recognition", default="input")
    args = parser.parse_args(sys.argv[1:]);
    FRGraph = FaceRecGraph();
    MTCNNGraph = FaceRecGraph();
    aligner = AlignCustom();
    extract_feature = FaceFeature(FRGraph)
    face_detect = MTCNNDetect(MTCNNGraph, scale_factor=2); #scale_factor, rescales image for faster detection
    main(args);
