# NetworkAudio
Transmits sound of a Windows based PC to another Windows based PC.
Tested only using local network, between two cumputers which are in the same subnet (gigabit connection between the two PCs).

This is a console application. You can use the description below, or just run my batch files from the folder "bin".

Have fun!

Usage as receiver: NetworkAudio.exe -r [TCPPort] [UDPPort]
Default ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)

Usage as sender: NetworkAudio.exe -s <IP Address> [TCPPort] [UDPPort]
Default ports are: 50001 for TCP (control) and 50000 for UDP (audio stream)
