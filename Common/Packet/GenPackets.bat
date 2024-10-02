START ../../PacketGenerator/bin/PacketGenerator.exe ../../PacketGenerator/PDL.xml
XCOPY /Y GenPackets.cs "../../DummyClient/Packet"
XCOPY /Y GenPackets.cs "../../Learnig_Server/Packet"
XCOPY /Y GenPackets.cs "../../Client/Assets/Scripts/Packet"

XCOPY /Y ClientPacketManager.cs "../../DummyClient/Packet"
XCOPY /Y ClientPacketManager.cs "../../Client/Assets/Scripts/Packet"
XCOPY /Y SeverPacketManager.cs "../../Learnig_Server/Packet"