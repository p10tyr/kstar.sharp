# a KStar Inverter data retriever from WiFi
Hybrid KSE4000 with WiFi, EZMeter and Battery array

Since 2017 it has undergone many forms and variations but I think the most stable version of this is
- Collect data (console app or docker)
- Send over MQTT (Ideally to HomeAssistant and use the Native Energy Dashboard)

As of 2023 anything that I dont use has been deleted and the rest has been upgraded to .NET6

## Currently this Repository has these functions

 - Core inverter UDP communication read only
 - Parsing bytes/hex to C# Object Models
 

## Disclaimer

The protocol has been reverse engineered and the goal of this code is for read only use only. 
I cannot guarantee it will work with all models and I cannot guarantee stability
For your safety this repository will never implement setting/writting anything back to the inverter.
Please use the official kstar app for any maintanence required on your inverter.


## Tested Inverters

Hybrid KSE4000 with WiFi, EZMeter and 10kW Battery array - Stable

Need help adding other kstar inverters.
I hope I can add other manufacturers too maybe - we'll see what happens


# License
 - This source code has been made available for educational uses only but is NOT licensed as any version of "Educational Community License"
 - Reverse engineering of some protocols were accomplished by monitoring unencrypted data within a sandboxed, private network. No attempts of hacking, brute force or use of non-public or leaked code was used. A jar file was eventually decompiled to help stich all the nonesense toghether. 
 - Source Code is released under the "fair use" policy, to allow for interobility on various other platforms, but only in a limited form. 
 - The limitation is read only data collection to allow to create monitoring software for personal use
 - No further attempt to fully reverse engineer the protocol will be made. eg, to allow sending data back to hardware.
 - At no point was the main contributor under any NDA agreement.
 - Reusuing some parts of this source code in any capacity may violate copy right law
 - Redistribution of this software in source or binary forms shall be free of all charges or fees to the recipient of this software.
 - All 3rd party licenses must be adhered too.


