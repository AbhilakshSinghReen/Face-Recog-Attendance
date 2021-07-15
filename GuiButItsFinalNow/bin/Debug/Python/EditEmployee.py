import mysql.connector
import os

#os.system('DbManagement.py')

dirname = os.path.dirname(__file__)
os.system("python "+os.path.join(dirname,'DbManagement.py'))

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



fh=open(os.path.join(dirname,'EditEmployee/Details.txt'),'r')
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
Ds=DOB.split(" ")
DOB=Ds[0]
Dates=DOB.split("-")
DOB=Dates[2]+"-"+Dates[1]+"-"+Dates[0]
DOB=DOB.replace(" ","")


command="UPDATE employees SET Title='"+str(Title)+"',Full_Name='"+str(FullName)+"',Date_Of_Birth='"+str(DOB)+"',Gender='"+str(Gender)+"',Employment_Status='"+str(EmpStatus)+"',Mobile_1='"+str(Mobile1)+"',Mobile_2='"+str(Mobile2)+"',Work_Email='"+str(WorkEmail)+"',Address='"+str(Address)+"' WHERE ID='"+str(Id)+"';"

command =command.replace("\n","")
print(command)
MainCursor.execute(command)

command="UPDATE people SET Name='"+str(FullName)+"' WHERE ID='"+str(Id)+"';"

command =command.replace("\n","")
print(command)
MainCursor.execute(command)

MainDB.commit()
