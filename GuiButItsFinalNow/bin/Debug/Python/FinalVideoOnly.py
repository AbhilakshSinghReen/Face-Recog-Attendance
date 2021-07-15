
import cv2
import copy
import os
import ctypes
import win32process
import time
import numpy as np
import onnx
import vision.utils.box_utils_numpy as box_utils
from caffe2.python.onnx import backend

# onnx runtime
import onnxruntime as ort

from win32api import GetSystemMetrics

"""
hwnd = ctypes.windll.kernel32.GetConsoleWindow()
if hwnd != 0:
    ctypes.windll.user32.ShowWindow(hwnd, 0)
    ctypes.windll.kernel32.CloseHandle(hwnd)
    _, pid = win32process.GetWindowThreadProcessId(hwnd)
    os.system('taskkill /PID ' + str(pid) + ' /f')
#"""

dirname = os.path.dirname(__file__)

NumberOfCameras=0
Cameras=[]
Frames=[]

#k123=cv2.VideoCapture('rtsp://admin:Password!@192.168.1.101/H.264/media.smp')
#Cameras.append(k123)

fh=open(os.path.join(dirname,'Config/Cameras.txt'),'r')
CamerasContent=fh.readlines()
NewCounter=0

for Cc in CamerasContent:
    Cc=Cc.replace("\n","")
    NewCounter+=1;
    if(NewCounter==1):
        NumberOfCameras=int(Cc)
        print(NumberOfCameras)
    else:
        Datas=Cc.split(": ",1)
        #print("DEBUG")
        #print(Datas[1][-1:])
        #print("")
        if(Datas[0][:6]=="Camera" and not Datas[0][-6:]=="Access"):
            Cc=Datas[1]
            IsWebcam=False
            for i in range (0,5):
                try:
                    if((i)==int(Cc)):
                        IsWebcam=True
                except: pass
            if(IsWebcam):
                NewCam=cv2.VideoCapture(int(Cc))
            else:
                NewCam=cv2.VideoCapture(Cc)
            Cameras.append(NewCam)
            print("Camera added"+str(Cc))
fh.close()







fh=open(os.path.join(dirname,'Config/ScreenSize.txt'),'r')
ScreenSize=fh.readline()
fh.close

fh=open(os.path.join(dirname,'Config/AllFacesSaveLocation.txt'),'r')
AllFacesSaveLocation=str(fh.readline())
fh.close

AllFacesSaveLocation= AllFacesSaveLocation.replace("\n","")
AllFacesSaveLocation.replace(os.sep,"/")

file_ends_with = ".jpg"

files=os.listdir(AllFacesSaveLocation)
files.sort(key=lambda x: os.path.getmtime(os.path.join(AllFacesSaveLocation,x)),reverse=True)


try:
    FileNameParams=files[0].split("-")
    FileNameParams=FileNameParams[1].split(".")
    ImageNumber=int(FileNameParams[0])
except:
    ImageNumber = 0
#print(str(ImageNumber))
#exit()

#ImageNumber=FileNameParams.split(".")


ScreenSize=ScreenSize.replace("\n","")
print(ScreenSize)
if(ScreenSize=="240 X 270"):
    MainScreenBackgroundImage=cv2.imread(os.path.join(dirname,"BackgroundImages/Small.jpg"))
elif(ScreenSize=="480 X 540"):
    MainScreenBackgroundImage=cv2.imread(os.path.join(dirname,"BackgroundImages/Medium.jpg"))
elif(ScreenSize=="720 X 810"):
    MainScreenBackgroundImage=cv2.imread(os.path.join(dirname,"BackgroundImages/Large.jpg"))
elif(ScreenSize=="960 X 1080"):
    MainScreenBackgroundImage=cv2.imread(os.path.join(dirname,"BackgroundImages/VeryLarge.jpg"))
elif(ScreenSize=="Fullscreen"):
    MainScreenBackgroundImage=cv2.imread(os.path.join(dirname,"BackgroundImages/VeryLarge.jpg"))
    Width = GetSystemMetrics(0)
    Height = GetSystemMetrics(1)
    Hx,Wx,Cxs=MainScreenBackgroundImage.shape
    Fxx=Width/Wx
    Fyx=height/Hx

    MainScreenBackgroundImage=cv2.resize(MainScreenBackgroundImage,None,fx=Fxx,fy=Fyx)
else:
    MainScreenBackgroundImage=cv2.imread(os.path.join(dirname,"BackgroundImages/VeryLarge.jpg"))








TwoFrameMargin=20
ThreeFrameMargin=20
FourFrameMargin=20

def AddFourFramesAndOneTableOnOutputFrame(OutputtFrame,Frame1,Frame2,Frame3,Frame4):
    OutputFrame=copy.copy(OutputtFrame)
    Height,Width,Channels=OutputFrame.shape

    CentreX=int(Width/2)
    CentreY=int(Height/2)

    try:
        H1f,W1f,Cs1f=Frame1.shape
        Fx1=(Width/2-2*FourFrameMargin)/W1f
        Fy1=((Height/2)-2*FourFrameMargin)/H1f

        if(Fx1<=Fy1):
            Frame1=cv2.resize(Frame1,None,fx=Fx1,fy=Fx1)
        else:
            Frame1=cv2.resize(Frame1,None,fx=Fy1,fy=Fy1)

        H1fm,W1fm,Cs1fm=Frame1.shape
        OutputFrame[CentreY-FourFrameMargin-H1fm:CentreY-FourFrameMargin,CentreX-FourFrameMargin-W1fm:CentreX-FourFrameMargin]=Frame1
    except:
        pass


    try:
        H2f,W2f,Cs2f=Frame2.shape
        Fx2=(Width/2-2*FourFrameMargin)/W2f
        Fy2=((Height/2)-2*FourFrameMargin)/H2f

        if(Fx2<=Fy2):
            Frame2=cv2.resize(Frame2,None,fx=Fx2,fy=Fx2)
        else:
            Frame2=cv2.resize(Frame2,None,fx=Fy2,fy=Fy2)

        H2fm,W2fm,Cs2fm=Frame2.shape
        OutputFrame[CentreY-FourFrameMargin-H2fm:CentreY-FourFrameMargin,CentreX+FourFrameMargin:CentreX+FourFrameMargin+W2fm]=Frame2

    except:
        pass

    try:
        H3f,W3f,Cs3f=Frame3.shape
        Fx3=(Width/2-2*FourFrameMargin)/W3f
        Fy3=((Height/2)-2*FourFrameMargin)/H3f

        if(Fx3<=Fy3):
            Frame3=cv2.resize(Frame3,None,fx=Fx3,fy=Fx3)
        else:
            Frame3=cv2.resize(Frame3,None,fx=Fy3,fy=Fy3)

        H3fm,W3fm,Cs3fm=Frame3.shape
        OutputFrame[CentreY+FourFrameMargin:CentreY+FourFrameMargin+H3fm,CentreX-FourFrameMargin-W3fm:CentreX-FourFrameMargin]=Frame3

    except:
        pass

    try:
        H4f,W4f,Cs4f=Frame4.shape
        Fx4=(Width/2-2*FourFrameMargin)/W4f
        Fy4=((Height/2)-2*FourFrameMargin)/H4f

        if(Fx4<=Fy4):
            Frame4=cv2.resize(Frame4,None,fx=Fx4,fy=Fx4)
        else:
            Frame4=cv2.resize(Frame4,None,fx=Fy4,fy=Fy4)

        H4fm,W4fm,Cs4fm=Frame4.shape
        OutputFrame[CentreY+FourFrameMargin:CentreY+FourFrameMargin+H4fm,CentreX+FourFrameMargin:CentreX+FourFrameMargin+W3fm]=Frame4
    except:
        pass
    return OutputFrame



def AddOneFrameAndOneTableOnOutputFrame(OutputtFrame,FrameToAdd):
    OutputFrame=copy.copy(OutputtFrame)
    Height,Width,Channels=OutputFrame.shape
    try:
        Hf,Wf,Csf=FrameToAdd.shape
        Fx=(Width)/Wf
        FrameToAdd=cv2.resize(FrameToAdd,None,fx=Fx,fy=Fx)
        Hfm,Wfm,Csfm=FrameToAdd.shape
    except:
        pass

    try:
        OutputFrame[int(Height/2-Hfm/2):int(Height/2-Hfm/2)+Hfm,0:int(Wfm)]=FrameToAdd
    except:
        pass

    return OutputFrame


def AddTwoFramesAndOneTableOnOutputFrame(OutputtFrame,Frame1,Frame2):
    OutputFrame=copy.copy(OutputtFrame)
    Height,Width,Channels=OutputFrame.shape


    try:
        H1f,W1f,Cs1f=Frame1.shape
        Fx1=(Width)/W1f
        Fy1=((Height/2)-2*TwoFrameMargin)/H1f

        if(Fx1<=Fy1):
            Frame1=cv2.resize(Frame1,None,fx=Fx1,fy=Fx1)
        else:
            Frame1=cv2.resize(Frame1,None,fx=Fy1,fy=Fy1)

        H1fm,W1fm,Cs1fm=Frame1.shape
    except:
        pass


    try:
        H2f,W2f,Cs2f=Frame2.shape
        Fx2=(Width)/W2f
        Fy2=((Height/2)-2*TwoFrameMargin)/H2f

        if(Fx2<=Fy2):
            Frame2=cv2.resize(Frame2,None,fx=Fx2,fy=Fx2)
        else:
            Frame2=cv2.resize(Frame2,None,fx=Fy2,fy=Fy2)

        H2fm,W2fm,Cs2fm=Frame2.shape
    except:
        pass


    try:
        OutputFrame[int(Height/2-TwoFrameMargin-H1fm):int(Height/2-TwoFrameMargin-H1fm)+int(H1fm),int(Width/2-W1fm/2):int(Width/2-W1fm/2+W1fm)]=Frame1
    except:
        pass
    try:
        OutputFrame[int(Height/2+TwoFrameMargin):int(Height/2+TwoFrameMargin)+int(H2fm),int(Width/2-W2fm/2):int(Width/2-W2fm/2+W2fm)]=Frame2
    except:
        pass

    return OutputFrame

def AddThreeFramesAndOneTableOnOutputFrame(OutputtFrame,Frame1,Frame2,Frame3):
    OutputFrame=copy.copy(OutputtFrame)
    Height,Width,Channels=OutputFrame.shape


    try:
        H1f,W1f,Cs1f=Frame1.shape
        Fx1=(Width)/W1f
        Fy1=((Height/3)-2*ThreeFrameMargin)/H1f

        if(Fx1<=Fy1):
            Frame1=cv2.resize(Frame1,None,fx=Fx1,fy=Fx1)
        else:
            Frame1=cv2.resize(Frame1,None,fx=Fy1,fy=Fy1)

        H1fm,W1fm,Cs1fm=Frame1.shape
    except:
        pass

    try:
        H2f,W2f,Cs2f=Frame2.shape
        Fx2=(Width)/W2f
        Fy2=((Height/3)-2*ThreeFrameMargin)/H2f

        if(Fx2<=Fy2):
            Frame2=cv2.resize(Frame2,None,fx=Fx2,fy=Fx2)
        else:
            Frame2=cv2.resize(Frame2,None,fx=Fy2,fy=Fy2)

        H2fm,W2fm,Cs2fm=Frame2.shape
    except:
        pass

    try:
        H3f,W3f,Cs3f=Frame3.shape
        Fx3=(Width)/W3f
        Fy3=((Height/3)-2*ThreeFrameMargin)/H3f

        if(Fx3<=Fy3):
            Frame3=cv2.resize(Frame3,None,fx=Fx3,fy=Fx3)
        else:
            Frame3=cv2.resize(Frame3,None,fx=Fy3,fy=Fy3)

        H3fm,W3fm,Cs3fm=Frame3.shape
    except:
        pass

    try:
        OutputFrame[int(Height/2 - H2fm/2 - ThreeFrameMargin -H1fm):int(Height/2 - H2fm/2 - ThreeFrameMargin -H1fm)+H1fm,int(Width/2 - W1fm/2):int(Width/2 - W1fm/2)+W1fm]=Frame1
    except:
        pass
    try:
        OutputFrame[int(Height/2 - H2fm/2):int(Height/2 - H2fm/2)+H2fm,int(Width/2 - W2fm/2):int(Width/2 - W2fm/2)+W2fm]=Frame2
    except:
        pass
    try:
        OutputFrame[int(Height/2 + H2fm/2 + ThreeFrameMargin):int(Height/2 + H2fm/2 + ThreeFrameMargin)+H3fm,int(Width/2 - W3fm/2):int(Width/2 - W3fm/2)+W3fm]=Frame3
    except:
        pass

    return OutputFrame


def ConcatenateFrames(FramesToConcatenate):
    OutputFrame=copy.copy(MainScreenBackgroundImage)
    Height,Width,Channels=OutputFrame.shape
    Length=len(FramesToConcatenate)


    if(Length==1):
        Frem= AddOneFrameAndOneTableOnOutputFrame(MainScreenBackgroundImage,FramesToConcatenate[0][1])
        #print("calling 1")
        return Frem

    elif(Length==2):
        Frem=AddTwoFramesAndOneTableOnOutputFrame(MainScreenBackgroundImage,FramesToConcatenate[0][1],FramesToConcatenate[1][1])
        #print("calling 2")
        return Frem

    elif(Length==3):
        Frem = AddThreeFramesAndOneTableOnOutputFrame(MainScreenBackgroundImage,FramesToConcatenate[0][1],FramesToConcatenate[1][1],FramesToConcatenate[2][1])
        #print("calling 3")
        return Frem

    elif(Length==4):
        Frem=AddFourFramesAndOneTableOnOutputFrame(MainScreenBackgroundImage,FramesToConcatenate[0][1],FramesToConcatenate[1][1],FramesToConcatenate[2][1],FramesToConcatenate[3][1])
        #print("calling 4")
        return Frem

############################################## STart



def predict(width, height, confidences, boxes, prob_threshold, iou_threshold=0.5, top_k=-1):
    boxes = boxes[0]
    confidences = confidences[0]
    picked_box_probs = []
    picked_labels = []
    for class_index in range(1, confidences.shape[1]):
        probs = confidences[:, class_index]
        mask = probs > prob_threshold
        probs = probs[mask]
        if probs.shape[0] == 0:
            continue
        subset_boxes = boxes[mask, :]
        box_probs = np.concatenate([subset_boxes, probs.reshape(-1, 1)], axis=1)
        box_probs = box_utils.hard_nms(box_probs,
                                       iou_threshold=iou_threshold,
                                       top_k=top_k,
                                       )
        picked_box_probs.append(box_probs)
        picked_labels.extend([class_index] * box_probs.shape[0])
    if not picked_box_probs:
        return np.array([]), np.array([]), np.array([])
    picked_box_probs = np.concatenate(picked_box_probs)
    picked_box_probs[:, 0] *= width
    picked_box_probs[:, 1] *= height
    picked_box_probs[:, 2] *= width
    picked_box_probs[:, 3] *= height
    return picked_box_probs[:, :4].astype(np.int32), np.array(picked_labels), picked_box_probs[:, 4]


label_path = "models/voc-model-labels.txt"

onnx_path = "models/onnx/version-RFB-320.onnx"
class_names = [name.strip() for name in open(label_path).readlines()]

predictor = onnx.load(onnx_path)
onnx.checker.check_model(predictor)
onnx.helper.printable_graph(predictor.graph)
predictor = backend.prepare(predictor, device="CPU")  # default CPU

ort_session = ort.InferenceSession(onnx_path)
input_name = ort_session.get_inputs()[0].name

#cap = cv2.VideoCapture("rtsp://admin:Password!@192.168.1.101/H.264/media.smp")  # capture from camera
cap = cv2.VideoCapture(0)  # capture from camera

threshold = 0.7

sum = 0
#ImageNumber =0
alt = 1


############################################# End


while True:
    Frames.clear()
    Count=0
    for Camera in Cameras:
        Count+=1
        ret,Frame=Camera.read()
        #cv2.imshow("hellu",Frame)
        FrameAndCamNo=(Count,Frame)

######################################### Recog STart

        alt+=1
        if Frame is None:
            print("no img")
            break

        #print("Saving"+str(alt))
        image = cv2.cvtColor(Frame, cv2.COLOR_BGR2RGB)
        image = cv2.resize(image, (320, 240))
        # image = cv2.resize(image, (640, 480))
        image_mean = np.array([127, 127, 127])
        image = (image - image_mean) / 128
        image = np.transpose(image, [2, 0, 1])
        image = np.expand_dims(image, axis=0)
        image = image.astype(np.float32)
        # confidences, boxes = predictor.run(image)
        time_time = time.time()
        confidences, boxes = ort_session.run(None, {input_name: image})
        print("cost time:{}".format(time.time() - time_time))
        boxes, labels, probs = predict(Frame.shape[1], Frame.shape[0], confidences, boxes, threshold)
        if(alt%6==0):
            alt=1
            for i in range(boxes.shape[0]):
                box = boxes[i, :]
                label = f"{class_names[labels[i]]}: {probs[i]:.2f}"

                cv2.rectangle(Frame, (box[0], box[1]), (box[2], box[3]), (255, 255, 0), 4)
                img_to_save = Frame[box[1]:box[3],box[0]:box[2]]
                ImageName=str(Count)+"-"+str(ImageNumber)+".jpg"
                try:
                    cv2.imwrite("temp_images/"+ImageName,img_to_save)
                    cv2.imwrite("AllFaces/"+ImageName,img_to_save)
                    cv2.imwrite(os.path.join(AllFacesSaveLocation,ImageName),img_to_save)
                except:
                    print("save error")
                print(str(os.path.join(AllFacesSaveLocation,ImageName)))
                ImageNumber+=1
                # cv2.putText(Frame, label,
                #             (box[0] + 20, box[1] + 40),
                #             cv2.FONT_HERSHEY_SIMPLEX,
                #             1,  # font scale
                #             (255, 0, 255),
                #             2)  # line type
        sum += boxes.shape[0]

######################################### Recog end


        Frames.append(FrameAndCamNo)

    FinalFrame=ConcatenateFrames(Frames)
    try:
        cv2.imshow("Cameras",FinalFrame)
    except:
        print("Print Error")

    if cv2.waitKey(20) == ord('q'):
        break

cv2.destroyAllWindows()
