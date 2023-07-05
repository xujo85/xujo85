from tkinter import *
import tkinter
import random
import time
from os import environ



xbg = "gray95" #bg
xfg = "#652828" #fg

bg = "white" #bg
fg = "#652828" #fg

window = tkinter.Tk()
window.title("Password Generator")
window.configure(bg="#FFFFFF")#pozadi

#window.wm_attributes('-alpha','#FFFFFF')





# specify the size of the window


#######################################
if 'ANDROID_BOOTLOGO' in environ:
   print("Android")
   check_system = "Android"
   window.geometry('380x612')
   bg2 = PhotoImage(file = "img/bg.png")

   #bg2 = bg2.subsample(3, 3) #zmensit
   bg2 = bg2.zoom(1, 1) #zoom
   label2 = Label( window, image = bg2)

   label2.place(x = -75, y = 450)#umiestnenie obrazka android

else:
   print("non-Android")
   check_system = "non-Android"
   window.geometry('380x900')
   bg2 = PhotoImage(file = "img/bg.png")

   bg2 = bg2.subsample(3, 3) #zmensit
   #bg2 = bg2.zoom(1, 1) #zoom
   label2 = Label( window, image = bg2)

   label2.place(x = 0, y = 0)#umiestnenie obrazka pc
#######################################


# condition variables
cond1 = IntVar()
cond2 = IntVar()
cond3 = IntVar()
cond4 = IntVar()
cond5 = IntVar()
cond6 = IntVar()
cond7 = IntVar()
length = IntVar()
qqq = IntVar()
#top labels
label_1 = tkinter.Label(window, text="Password Generator", font=("ComicSansMS", 12))
label_1.pack()#nadpis

label_2 = tkinter.Label(window, text="Select your options", font=("ComicSansMS", 10))
label_2.pack()#select

# charecter lists
male = "abcdefghijklmnopqrstuvwxyz"
velke = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
cislice = "1234567890"
special = "!?@#_&*:=+-"
special2 = "€;(÷)<>{}[]$§^%£"
unsafe = "¡¿¬™'°\"®/\\©¶֍π▒밿␌"

fulllist = (male)+(velke)+(cislice)+(special)+(special2)+(unsafe)+(" ")

list_1 = list(male)
list_2 = list(velke)
list_3 = list(special)
list_4 = list(cislice)
list_5 = list(special2)
list_6 = [' ',] # space
list_7 = list(unsafe)

list_8 = list(fulllist)
print(fulllist)
random.shuffle(list_8)
allstring = ''.join(list_8)
print(f"randomized: {allstring}")

def call_warning():
    label_2.config(text='Norhing selected!', bg="red")
    label_2.update()
    time.sleep(0.5)
    label_2.config(text='Select options !', bg="white")
    label_2.update()   

# function to generate password
def password():
    final_list = []
    if (cond3.get()):
        final_list.append(list_1)
    if (cond4.get()):
        final_list.append(list_2)
    if (cond2.get()):
        final_list.append(list_3)
    if (cond1.get()):
        final_list.append(list_4)
    if (cond5.get()):
        final_list.append(list_5)
    if (cond6.get()):
        final_list.append(list_6)
    if (cond7.get()):
        final_list.append(list_7)
    bound = cond1.get() + cond2.get() + cond3.get() + cond4.get() + cond5.get() + cond6.get() + cond7.get()
    #bound = final_list[0] + final_list[1] + final_list[2] + final_list[3] + final_list[4] + final_list[5] + final_list[6]
    ln = length.get()
    #print(ln)
    #print(final_list)
    #print(bound)
    if (bound==0):
        print("nic si neoznacil !")
        call_warning()
        return ("")
    password = []
    val = ""
    for i in range(ln):
        a = random.randint(0, bound-1)
        #print(len(final_list[a-1]))
        print(final_list[a][random.randint(0,len(final_list[a]))-1]) 
        val =  final_list[a][random.randint(0,len(final_list[a]))-1]
        password.append(val)
        #val = ""
        #pswrd = ""  
    return (''.join(password))

# gloabal password variable
pswrd = StringVar()
label_4 = StringVar()
pswrd.set(password())
qqq = StringVar()
qqq.set(password())


# function to display generated password
def gen_password_line():
    #global txt_1
    #txt_1.pack_forget()
    pswrd.set(password())
    txt_1 = tkinter.Label(window, textvariable=pswrd, font=("ComicSansMS", 18))
    #text.delete('1.0', END) #zmaze hesla
    text.insert('1.0', (pswrd.get()) + "\n")
    text.pack()
    #button_2.pack() #copy last clipboard
    #button_3.pack(side = TOP, expand = True, fill = BOTH)#copy all clipboard
    label_4.pack(side = TOP, expand = True, fill = BOTH) # system is: ???

def gen_password_plus():
    #label2.destroy()#zmaze obrazok
    #global txt_1
    #txt_1.pack_forget()
    pswrd.set(password())
    #txt_1 = tkinter.Label(window, textvariable=pswrd, font=("ComicSansMS", 18))
    #text.delete('1.0', END) #zmaze hesla
    text.insert('1.0', (pswrd.get()))
    text.pack()
    #button_2.pack() #copy last clipboard
    #button_3.pack(side = TOP, expand = True, fill = BOTH)#copy all clipboard
    label_4.pack(side = TOP, expand = True, fill = BOTH) # system is: ???


##### funkcia clipboardu
def copy_one_clipboard():
    window.clipboard_clear()
    window.clipboard_append(pswrd.get())
    #label2.destroy()
    #window.update()
    
def copy_all_clipboard():
    window.clipboard_clear()
    window.clipboard_append(text.get('1.0', END))
    
def window_clear():
	text.delete('1.0', END)

def none():
	print("none was trigered")
	pass

def imgdest():
	label2.destroy()


# uz nepotrebne
#strz1 = ''.join(list_3)
#strz2 = ''.join(list_5)
#strz3 = ''.join(list_7)

# check buttony
chkbutton_1 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Numbers', variable=cond1, onvalue=1, offvalue=0)

chkbutton_2 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Special  ' + str(special), variable=cond2, onvalue=1, offvalue=0)

chkbutton_3 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Small Letters', variable=cond3, onvalue=1, offvalue=0)

chkbutton_4 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Capital Letters', variable=cond4, onvalue=1, offvalue=0)

chkbutton_5 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Special2  ' + str(special2), variable=cond5, onvalue=1, offvalue=0)

chkbutton_6 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Space', variable=cond6, onvalue=1, offvalue=0)

chkbutton_7 = tkinter.Checkbutton(window,bg=(xbg), fg=(xfg), text='Unsafe  ' +  str(unsafe), variable=cond7, onvalue=1, offvalue=0)



# dlzka hesla
spinbox_1 = tkinter.Spinbox(window,font=('Ivy 16 bold'), from_=8, to_=336,textvariable=length,  width=6)

"""
#uz nepotrebujem
button_1 = tkinter.Button(window,bg=(bg), fg=(fg) ,font=('Ivy 12 bold'), text="Generate password", command=display_password)

button_2 = tkinter.Button(window,bg="gray70", text="Copy last pw to clipboard",command=copy_one_clipboard)

button_3 = tkinter.Button(window,bg="gray70", text="Copy all pw to clipboard", command=copy_all_clipboard)
"""



label_4 = tkinter.Label(window, text=(f"System is: " + (check_system)), font=("ComicSansMS", 3) ,fg="blue")






# run created components
chkbutton_1.pack() #numbers
chkbutton_4.pack() #capital
chkbutton_3.pack() #small
chkbutton_6.pack() #space
chkbutton_2.pack() #special
#chkbutton_5.pack() #special2
chkbutton_7.pack() #unsafe
spinbox_1.pack() #dlzka hesla
#button_1.pack() #generate pw


text = Text(window, height=38)
#text.insert('1.0', (pswrd.get()))

##############
# NEW BUTTONS #
##############

#############
# line 0  buttons #
#############
frame0 = Frame(window)
frame0.pack()
bottomframe0 = Frame(window)
bottomframe0.pack( side = BOTTOM )

gen = Button(frame0, text="Generate \\n", fg="green", bg="gray80", font=('Ivy 10 bold'), command=gen_password_line)
gen.pack( side = LEFT)

gen2 = Button(frame0, text="Generate +", fg="blue2", bg="gray80", font=('Ivy 10 bold'), command=gen_password_plus)
gen2.pack( side = LEFT)


#############
# line 1  buttons #
#############
frame1 = Frame(window)
frame1.pack()
bottomframe1 = Frame(window)
bottomframe1.pack( side = BOTTOM )

specialbutton1 = Button(frame1, text="Clear\nall", fg="red", bg="gray90", font=('Ivy 6 bold'),command=window_clear)
specialbutton1.pack( side = LEFT)

specialbutton2 = Button(frame1, text="L1\nb2", fg="brown", bg="gray90", font=('Ivy 6 bold'),command = none)
specialbutton2.pack( side = LEFT )

specialbutton3 = Button(frame1, text="L1\nb3", fg="blue1", bg="gray80", font=('Ivy 6 bold'),command = none)
specialbutton3.pack( side = LEFT )

specialbutton4 = Button(frame1, text="copy\nlast", fg="blue2", bg="gray70", font=('Ivy 6 bold'),command = copy_one_clipboard)
specialbutton4.pack( side = LEFT )

specialbutton5 = Button(frame1, text="copy\nall", fg="blue3", bg="gray60", font=('Ivy 6 bold'),command = copy_all_clipboard)
specialbutton5.pack( side = LEFT )

#############
# line 2 buttons #
#############
frame2 = Frame(window)
frame2.pack()
bottomframe2 = Frame(window)
bottomframe2.pack( side = BOTTOM )

specialbutton1 = Button(frame2, text="btn1", fg="red", font=('Ivy 6 bold'), command= none)
specialbutton1.pack( side = LEFT)

specialbutton2 = Button(frame2, text="btn2", fg="brown", bg="gray90", font=('Ivy 6 bold'),command = none)
specialbutton2.pack( side = LEFT )

specialbutton3 = Button(frame2, text="btn3", fg="blue1", bg="gray80", font=('Ivy 6 bold'),command = none)
specialbutton3.pack( side = LEFT )

specialbutton4 = Button(frame2, text="call_warn", fg="blue2", bg="gray70", font=('Ivy 6 bold'),command = call_warning)
specialbutton4.pack( side = LEFT )

specialbutton5 = Button(frame2, text="img clr", fg="blue3", bg="gray60", font=('Ivy 6 bold'),command=imgdest)
specialbutton5.pack( side = LEFT )


##############

window.mainloop()
check_system = "unknown"