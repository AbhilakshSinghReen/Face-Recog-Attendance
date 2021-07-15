import time
from datetime import datetime,timedelta
import mysql.connector
import os
import cv2
import base64
import ctypes
import win32process
from imutils.video import FileVideoStream
from align_custom import AlignCustom
from face_feature import FaceFeature
from mtcnn_detect import MTCNNDetect
from tf_graph import FaceRecGraph
import argparse
import sys
import json
import numpy as np
import numpy as np
import glob
#rom PIL import Image
import urllib.request

"""
hwnd = ctypes.windll.kernel32.GetConsoleWindow()
if hwnd != 0:
    ctypes.windll.user32.ShowWindow(hwnd, 0)
    ctypes.windll.kernel32.CloseHandle(hwnd)
    _, pid = win32process.GetWindowThreadProcessId(hwnd)
    os.system('taskkill /PID ' + str(pid) + ' /f')

#"""

#os.system('DbManagement.py')
dirname = os.path.dirname(__file__)
os.system("python "+os.path.join(dirname,'DbManagement.py'))

DummyImageForTesting=cv2.imread(os.path.join(dirname,"BackgroundImages\DummyHuman.jpg"))
############################################################################################################################# DB code starts here

fh=open(os.path.join(dirname,'Config/config.txt'),'r')
ImageNumber=int(fh.readline())
fh.close()
fh=open(os.path.join(dirname,'Config/host.txt'),'r')
HostName=str(fh.readline())
fh.close()
fh=open(os.path.join(dirname,'Config/user.txt'),'r')
UserName=str(fh.readline())
fh.close()
fh=open(os.path.join(dirname,'Config/password.txt'),'r')
Password=str(fh.readline())
fh.close()

fh=open(os.path.join(dirname,'Config/LogsTimePeriod.txt'),'r')
LogsDeltaSeconds=int(fh.readline())
fh.close()

HostName=HostName.replace("\n","")
UserName=UserName.replace("\n","")
Password=Password.replace("\n","")

MainDB= mysql.connector.connect(host=HostName,user=UserName,passwd=Password,auth_plugin='mysql_native_password')
MainCursor=MainDB.cursor(buffered=True)

try:
    MainCursor.execute("use face__recog")
except:
    print("hllu")

def SaveData():
    global ImageNumber
    MainDB.commit()
    fh=open(os.path.join(dirname,'Config/config.txt'),'w')
    fh.write(str(ImageNumber))
    fh.close()

def ResetConfig():
    fh=open(os.path.join(dirname,'Config/config.txt'),'w')
    fh.write(str(0))
    fh.close()

def EncodeImageToBytes(Image):
    Image=bytearray(Image)
    return Image

def ResizeImageTo50x50(Image):
    H,W,C=Image.shape
    Fx=50/W
    Fy=50/H
    Image=cv2.resize(Image,None,fx=Fx,fy=Fy)
    return Image

def GetDateTimeIn_DB_Format():
    TempTime=datetime.today().strftime('%Y-%m-%d-%H:%M:%S')
    Times=TempTime.split("-")
    TempTime=Times[3]
    if(TempTime=="00:00:00"):
        time.sleep(2)
    return datetime.today().strftime('%Y-%m-%d-%H:%M:%S')

def GetDateIn_DB_Format():
    return datetime.today().strftime('%Y-%m-%d')

def SaveImage(Image):
    global ImageNumber
    ImageNumber+=1
    cv2.imwrite("Images/"+str(ImageNumber)+".jpg",Image)
    return ImageNumber

def AddToDBMain(CameraNo,Image,Name,Status,ID,InDateTime,OutDateTime):
    print("Add to DB main called")
    Image=ResizeImageTo50x50(Image)
    values="'"+str(CameraNo)+"','"+str(SaveImage(Image))+"','"+str(Name)+"','"+str(Status)+"','"+str(ID)+"','"+str(InDateTime)+"','"+str(OutDateTime)+"'" #CCYY-MM-DD hh:mm:ssformat
    MainCursor.execute("insert into people values(NULL,"+values+")")
    SaveData()

def GetInTimeFor_ID_AndDate(ID,Date):
    MainCursor.execute("select intime from people where id ='"+str(ID)+"'")
    for i in MainCursor:

        i=str(i)
        i=i.replace("/","-")
        i=i.replace("(datetime.datetime(","")
        i=i.replace("),)","")
        i=i.replace(",","-")
        i=i.replace(" ","")
        f=i[0:10]
        TempDates=f.split("-")
        Year=TempDates[0]
        Month=TempDates[1]
        Day=TempDates[2]

        if(len(Month)==1):
            Month="0"+Month
        if(len(Day)==1):
            Day="0"+Day

        f=Year+"-"+Month+"-"+Day
        print("f=",str(f))
        print("d=",str(Date))

        if(str(f)==str(Date)):
            print(i)
            return i
        #else:
        #    print("NO DATE")

def ChangeOutDateTimeInDB(ID,CamNo,InDateTime,NewOutDateTime):
    MainCursor.execute("update people set outtime= '" + str(NewOutDateTime)+"' where id ='"+str(ID)+"' and InTime ='"+str(InDateTime)+"'")
    MainCursor.execute("update people set cam= '" + str(CamNo)+"' where id ='"+str(ID)+"' and InTime ='"+str(InDateTime)+"'")
    SaveData()
    print("changed")

def CheckIfIn_DB_For_ID_AndDate(Id,Date):
    MainCursor.execute("select id from people where id ='"+str(Id)+"' and InTime >'"+str(Date)+"'")
    for i in MainCursor:
        return True
    return False

def GetNameFrom_ID(ID):
    MainCursor.execute("select Full_Name from employees where id = '"+str(ID)+"'")
    for i in MainCursor:
        i=str(i)
        i=i.replace("'","")
        i=i.replace(")","")
        i=i.replace("(","")
        i=i.replace(",","")
        return i


LogDeltaTime= timedelta(seconds=LogsDeltaSeconds)

def GetLastLogTime(Id,CamNo):
    MainCursor.execute("select LogTime from logs where ID ='"+str(Id)+"' and Cam = '"+str(CamNo)+"' order by EntryNumber desc LIMIT 1")

    #print("GetLastLogTime : Executed " + str(len(MainCursor)))

    #if(len(MainCursor)==0):
    #    return "0"

    for i in MainCursor:
        print("Entered loop")
        i=str(i)
        i=i.replace("(datetime.datetime(","")
        i=i.replace("),)","")
        i=i.replace(" ","")
        return i

def CanAddToLogs(Id,CamNo,LogTime):
    LastLogTime=GetLastLogTime(Id,CamNo)
    if(str(LastLogTime).find(",") == -1):
        print("Can add")
        return True
    LogTime=LogTime.replace('-',',')
    LogTime=LogTime.replace(':',',')
    if(LastLogTime.count(",")<5):
        print("Can add")
        return True
    print("Last LogTime = " + LastLogTime)
    LastLogTime  = datetime.strptime(LastLogTime, "%Y,%m,%d,%H,%M,%S")
    LogTime  = datetime.strptime(LogTime, "%Y,%m,%d,%H,%M,%S")
    if((LogTime-LastLogTime) > LogDeltaTime):
        print("Can add")
        return True
    else:
        print("Can't add")
        return False


def AddToLogs(Id,CamNo,LogTime):
    if(Id==""):
        Id="UnKnown"
    if(CanAddToLogs(Id,CamNo,LogTime)):
        values="'"+str(Id)+"','"+str(GetNameFrom_ID(Id))+"','"+str(CamNo)+"','"+str(LogTime)+"'" #CCYY-MM-DD hh:mm:ssformat
        MainCursor.execute("insert into logs values(NULL,"+values+")")
        SaveData()
    else:
        print("Can't add, too early")


def AddOrChangeInDB(CamNo,Image,Id):
    AddToLogs(Id,CamNo,GetDateTimeIn_DB_Format())
    if not (CheckIfIn_DB_For_ID_AndDate(Id,GetDateIn_DB_Format())):
        print("Checking for 'None' Exception and adding")
        if not (GetNameFrom_ID(str(Id)).strip()=="None"):
            AddToDBMain(CamNo,Image,GetNameFrom_ID(str(Id)),"Present",Id,GetDateTimeIn_DB_Format(),GetDateTimeIn_DB_Format())
            print("Added")
        else:
            print("'None' Exception: Id = 'None'")
    else:
        ChangeOutDateTimeInDB(Id,CamNo,GetInTimeFor_ID_AndDate(Id,GetDateIn_DB_Format()),GetDateTimeIn_DB_Format())
        print("Changed")

############################################################################################################################### DB code ends here


url='http://192.168.1.101/cgi-bin/camera'

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
def camera_recog():
    print("[INFO] camera sensor warming up...")
#    vs = cv2.VideoCapture(0); #get input from webcam

    #vs = cv2.VideoCapture("C:\\Users\\Lavi\\Desktop\\vid1.mp4"); #get input from webcam

    #vs = cv2.VideoCapture("rtsp://admin:Password!@192.168.1.101/H.264/media.smp")
    #fvs = FileVideoStream("rtsp://admin:Password!@192.168.1.101/H.264/media.smp").start()
    alt = 0
    detect_time = time.time()

    img_dir = "temp_images" # Enter Directory of all images

#    img_dir = "C:/Users/Lavi/Desktop/NewFlask (1)/Images" # Enter Directory of all images
    #copy_dir = "C:/Users/Lavi/Desktop/NewFlask (1)/copied"

    data_path = os.path.join(img_dir,'*g')
    files = glob.glob(data_path)
    data = []
    while True:
        files = glob.glob(data_path)
        data = []
        for f1 in files:
            print(str(f1))
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
            f2=os.path.basename(f1)
            Names=f2.split('-')
            CameraNumber=Names[0]

            try:
                #u can certainly add a roi here but for the sake of a demo i'll just leave it as simple as this
                rects, landmarks = face_detect.detect_face(frame,30);#min face size is set to 80x80, distance, larger better
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
                            AddOrChangeInDB(CameraNumber,frame,str(recog_data[i][0]))
                            #AddOrChangeInDB(1,DummyImageForTesting,"101")

                    #alt+=3
                #alt-=1

                #frame = cv2.resize(frame,None,fx=0.5,fy=0.5)
                cv2.imshow("Frame",frame)
                print(str(f1))

                #imagetemp.save("C:\\Users\\Lavi\\Downloads\\Face-Track-Detect-Extract-master\\Face-Track-Detect-Extract-master\\facepics\\test")
                #if img_dir in f1:
                #    c2 = f1.replace(img_dir,'C:\\Users\\Lavi\\Downloads\\Face-Track-Detect-Extract-master\\Face-Track-Detect-Extract-master\\facepics\\test')
                #    print("True")

            #    os.chdir("C:\\Users\\Lavi\\Downloads\\Face-Track-Detect-Extract-master\\Face-Track-Detect-Extract-master\\facepics\\test")
            #    cv2.imwrite(c2,frame)
            #    print(f1)
            #    time.sleep(1)
                key = cv2.waitKey(1) & 0xFF
                if key == ord("q"):
                    break
            except:
                pass

            key = cv2.waitKey(1) & 0xFF
            if key == ord("q"):
                break
            try:
                os.remove(f1)
            #    print("True")
            except:
                print("Delete excption"+f1)

        key = cv2.waitKey(1) & 0xFF
        if key == ord("q"):
            break

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
def findPeople(features_arr, positions, thres = 0.6, percent_thres = 70):
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
def create_manual_data():
    vs = cv2.VideoCapture("C:/Users/Lavi/Desktop/Train_Sanjeev.mp4"); #get input from webcam
    #imgResp1=urllib.request.urlopen(url)
#    imgNp1=np.array(bytearray(imgResp1.read()),dtype=np.uint8)
#    img1=cv2.imdecode(imgNp1,-1)
    print("Please input new user ID:")
    new_name = input(); #ez python input()
    f = open('./facerec_128D.txt','r');
    data_set = json.loads(f.read());
    person_imgs = {"Left" : [], "Right": [], "Center": []};
    person_features = {"Left" : [], "Right": [], "Center": []};
    print("Please start turning slowly. Press 'q' to save and add this new user to the dataset");
    while True:
        #_, frame = vs.read();
        imgResp1=urllib.request.urlopen(url)
        imgNp1=np.array(bytearray(imgResp1.read()),dtype=np.uint8)
        img1=cv2.imdecode(imgNp1,-1)
        frame = img1
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





if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", type=str, help="Run camera recognition", default="camera")
    args = parser.parse_args(sys.argv[1:]);
    FRGraph = FaceRecGraph();
    MTCNNGraph = FaceRecGraph();
    aligner = AlignCustom();
    extract_feature = FaceFeature(FRGraph)
    face_detect = MTCNNDetect(MTCNNGraph, scale_factor=2); #scale_factor, rescales image for faster detection
    main(args);




#while True:
#    print("recogniser is running")
