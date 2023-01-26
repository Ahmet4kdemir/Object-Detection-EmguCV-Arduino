#include<Servo.h>
int Th1, Th2, tmp;
Servo M1,M2;

void setup() 
{
  Serial.begin(9600); //port rate = connection between computer and arduino
  pinMode(11,OUTPUT);
  digitalWrite(13,0);
  Th1 = 130;
  Th2 = 90;
  M1.attach(6);
  M2.attach(5);
  M1.write(Th1);
  M2.write(Th2);
}

void loop() 
{
  delay(200); //its important cuz  make computer slower than arduino to sync arduino and computer.C# should be slower than arduino

  if(Serial.available()>=2)
  {
    Th1 = Serial.read(); //read only one byte
    Th2 = Serial.read();

    //Remove any extra worng reading
    while(Serial.available()) tmp = Serial.read();

    // Run the robotic arm here. For testing, we will 
    M1.write(Th2);
    M2.write(Th1);
    digitalWrite(11,1);
    delay(50);
    //M1.wr
    //M2.wr
    //LED => No
    //delay
    //LED off
    //switch On or switch off a LED according to Th1 value
   


    //Serial.print('1'); // This tell the PC that Arduino is Ready for next angles
  }


}
