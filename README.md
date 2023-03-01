# a KStar Inverter data retriever from WiFi

Written in C# .NET6 - Can run any where with a console and connection to the Inverter over WiFi

Tested with Hybrid KSE4000 with WiFi, EZMeter and Battery array for past 6 years.


# Run Me

Pull the Docker image from  (Todo add link)
- todo create a simple free docker build pipeline
- todo caputre enviroment varables to configure

Build locally the console app
- todo ? a simple build file?

### IP_ADDRESS_INVERTER 
`--ip-`

If you run this as a console app in native OS it is allowed to send out a broadcast request and the inverter responds to that so you dont need this.
But it is much nicer running this in docker but sadly in docker you need to set the IP as broadbast doesnt work.
Set your Inverter to a static IP on your home Router (connect the inverter to your Wifi)


### MQTT_CONNECTION_STRING
`--mqtt-`

I think its just the hostname. I have `homeassistant`
- port 1833
- client id `kstar.sharp.console`
- user:pass `mqtt:mqtt`

Topics
- sensor/inverter/pvpower
- sensor/inverter/grid
- sensor/inverter/grid/import
- sensor/inverter/grid/export
- sensor/inverter/load
- sensor/inverter/temp
- sensor/inverter/etoday

Set as **AtMostOnceQoS**

### SILENT_MODE
`--silent-`

If you run this in an interactive console you will get a page update every x seconds.
In docker this is a lot of noise that gets logged.. we just want errors there and the messages to make thier way over MQTT



----

Some old stuff

Don't remember what this ia about as I wrote this a long time ago.. seems pretty cool though. (also deleted a lot of nonesense recenlty)

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

# History

Since 2017 it has undergone many forms and variations but I think the most stable version of this is
- Collect data (console app or docker)
- Send over MQTT (Ideally to HomeAssistant and use the Native Energy Dashboard)

As of 2023 anything that I dont use has been deleted and the rest has been upgraded to .NET6


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


