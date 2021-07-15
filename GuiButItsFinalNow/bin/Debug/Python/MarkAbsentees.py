import time
from datetime import datetime,timedelta, date
import mysql.connector
import os

dirname = os.path.dirname(__file__)
os.system("python "+os.path.join(dirname,'DbManagement.py'))

fh=open(os.path.join(dirname,'Config/MarkAbsenteesDate.txt'),'r')
LastDate=str(fh.readline())
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
HostName=HostName.replace("\n","")
UserName=UserName.replace("\n","")
Password=Password.replace("\n","")

MainDB= mysql.connector.connect(host=HostName,user=UserName,passwd=Password,auth_plugin='mysql_native_password')
MainCursor=MainDB.cursor(buffered=True)

try:
    MainCursor.execute("use face__recog")
except:
    print("hllu")

AllIds=[]

def GetNameFrom_ID(ID):
    MainCursor.execute("select Full_Name from employees where id = '"+str(ID)+"'")
    for i in MainCursor:
        i=str(i)
        i=i.replace("'","")
        i=i.replace(")","")
        i=i.replace("(","")
        i=i.replace(",","")
        return i

def GetDateIn_DB_Format():
    return datetime.today().strftime('%Y-%m-%d')

def CheckIfIn_DB_For_ID_AndDate(Id,Date1,Date2):
    MainCursor.execute("select id from people where id ='"+str(Id)+"' and InTime >='"+str(Date1)+"' and InTime <='"+str(Date2)+"'")
    for i in MainCursor:
        return True
    return False

def MarkAbsent(ID,Date):
    values="'"+str("0")+"','"+str("0")+"','"+str(GetNameFrom_ID(ID))+"','"+str("Absent")+"','"+str(ID)+"','"+str(Date)+"','"+str(Date)+"'"
    MainCursor.execute("insert into people values(NULL,"+values+")")
    MainDB.commit()


LastDate=datetime.strptime(LastDate,'%Y-%m-%d').date()

TodayDate=datetime.strptime(GetDateIn_DB_Format(),'%Y-%m-%d').date()
EndDate=TodayDate-timedelta(1)

#print(TodayDate)
#print(EndDate)







def MarkAbsentees():

    for n in range(int ((EndDate - LastDate).days)):
        TargetDate= LastDate + timedelta(n)
        OtherTargetDate=TargetDate+ timedelta(1)

        MainCursor.execute("select ID from employees")
        for Id in MainCursor:
            Id=str(Id)
            Id=Id.replace("'","")
            Id=Id.replace("(","")
            Id=Id.replace(")","")
            Id=Id.replace(",","")
            AllIds.append(Id)

        for Id in AllIds:
            if not CheckIfIn_DB_For_ID_AndDate(Id,TargetDate.strftime('%Y-%m-%d'),OtherTargetDate.strftime('%Y-%m-%d')):
                MarkAbsent(Id,TargetDate)
                print("Marking ",str(Id)," absent on ",str(TargetDate))




MarkAbsentees()

fh=open(os.path.join(dirname,'Config/MarkAbsenteesDate.txt'),'w')
fh.write(EndDate.strftime('%Y-%m-%d'))
fh.close()
