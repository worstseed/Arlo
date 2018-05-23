#include "simpletools.h"
#include "arlodrive.h"
#include "ping.h"
  
int n = 0, pingPin, cmDist;
int pins[4] = {14, 16, 17, 15};

int main()
{
  while(1)
  {
   dhb10_terminal(SIDE_TERM);
   print("Back in main program.");
   
   while(n < 4)
   {
     pingPin = pins[n];
     cmDist = ping_cm(pingPin);
     switch(pingPin)
     {
        case 14: print("Front: "); break;
        case 16: print("Left:  "); break;
        case 17: print("Right: "); break;
        case 15: print("Back:  "); break;
      }
    
      print("%03d cm\n", cmDist);
      n++; 
    }
    if(n >= 4) n = 0; 
  }  
}