import mysql.connector
import os
import cv2

#Set this to decide whether or not image is being used
IsUsingNewEmployeeImage=False

dirname = os.path.dirname(__file__)
os.system("python "+os.path.join(dirname,'DbManagement.py'))
fh=open(os.path.join(dirname,'Config/Empconfig.txt'),'r')
EmpImageNumber=int(fh.read())
fh.close()

fh=open(os.path.join(dirname,'NewEmployee/Training.txt'),'r')
TrainingType=fh.readline()
TrainginParams=fh.readline()
fh.close()

fh=open(os.path.join(dirname,'Config/host.txt'),'r')
HostName=str(fh.read())
fh.close()
fh=open(os.path.join(dirname,'Config/user.txt'),'r')
UserName=str(fh.read())
fh.close()
fh=open(os.path.join(dirname,'Config/password.txt'),'r')
Password=str(fh.read())
fh.close()

HostName=HostName.replace("\n","")
UserName=UserName.replace("\n","")
Password=Password.replace("\n","")

MainDB= mysql.connector.connect(host=HostName,user=UserName,passwd=Password,auth_plugin='mysql_native_password')
MainCursor=MainDB.cursor(buffered=True)
MainCursor.execute("use face__recog")

def SaveData():
    #global EmpImageNumber
    MainDB.commit()

    #fh=open(os.path.join(dirname,'Config/Empconfig.txt'),'w')
    #fh.write(str(EmpImageNumber))
    #fh.close()

fh=open(os.path.join(dirname,'NewEmployee/Details.txt'),'r')
Id=fh.readline()
Title=fh.readline()
FullName=fh.readline()
DOB=fh.readline()
Gender=fh.readline()
EmpStatus=fh.readline()
Mobile1=fh.readline()
Mobile2=fh.readline()
WorkEmail=fh.readline()
Address=fh.readline()
fh.close()

DOB=DOB.replace("\n", "")
DOB=DOB.replace("/","-")
Ds=DOB.split(" ")
DOB=Ds[0]
Dates=DOB.split("-")
DOB=Dates[2]+"-"+Dates[1]+"-"+Dates[0]
DOB=DOB.replace(" ","")

values="'"+str(Id)+"','"+str(Title)+"','"+str(FullName)+"','"+str(DOB)+"','"+str(Gender)+"','"+str(EmpStatus)+"','"+str(Mobile1)+"','"+str(Mobile2)+"','"+str(WorkEmail)+"','"+str(Address)+"'"

values=values.replace("\n", "")

values=values.replace(" '", "'")

#print(str(values))

TrainingSuccess=True
TrainingType=TrainingType.replace("\n","")


if(TrainingType=="Camera"):
    os.system("python "+os.path.join(dirname,'save1.py'))
elif(TrainingType=="Video"):
    os.system("python "+os.path.join(dirname,'save3.py'))
elif(TrainingType=="Images"):
    os.system("python "+os.path.join(dirname,'save2.py'))

Id=Id.replace("\n","")


if(TrainingSuccess):
    MainCursor.execute("insert into employees values("+str(values)+")")
    SaveData()
    if(IsUsingNewEmployeeImage):
        Img=cv2.imread("NewEmployee/Image.jpg")
        path=os.path.join(dirname,"EmployeeImages/"+str(Id)+".jpg")
        print(path)
        cv2.imwrite(path,Img)
        os.remove("NewEmployee/Image.jpg")
