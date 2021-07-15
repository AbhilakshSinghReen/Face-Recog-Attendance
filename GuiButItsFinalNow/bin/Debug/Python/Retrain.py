import os
dirname = os.path.dirname(__file__)

fh=open(os.path.join(dirname,'Retrain/RetrainDetails.txt'),'r')
TrainingType=fh.readline()
TrainingParams=fh.readline()
EId=fh.readline()
fh.close()

TrainingType=TrainingType.strip()
TrainingParams=TrainingParams.strip()
EId=EId.strip()


if(TrainingType=="Camera"):
    print("Camera")
    os.system("python "+os.path.join(dirname,'RetrainByCamera.py'))
elif(TrainingType=="Video"):
    print("video")
    os.system("python "+os.path.join(dirname,'RetrainByVideo.py'))
elif(TrainingType=="Images"):
    print("images")
    os.system("python "+os.path.join(dirname,'RetrainByImages.py'))
