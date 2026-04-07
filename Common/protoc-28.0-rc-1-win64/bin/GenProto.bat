@echo off
echo [1/3] Running protoc compiler...
protoc.exe -I=./ --csharp_out=./ ./Protocol.proto 
IF ERRORLEVEL 1 (
    echo ERROR: protoc compilation failed!
    PAUSE
    EXIT /B 1
)

echo [2/3] Running PacketGenerator...
START /WAIT ../../../Server/PacketGenerator/bin/PacketGenerator.exe ./Protocol.proto
IF ERRORLEVEL 1 (
    echo ERROR: PacketGenerator failed!
    PAUSE
    EXIT /B 1
)

echo [3/3] Copying generated files...
XCOPY /Y Protocol.cs "../../../Client/Assets/Scripts/Packet"
XCOPY /Y Protocol.cs "../../../Server/Server/Packet"
XCOPY /Y ClientPacketManager.cs "../../../Client/Assets/Scripts/Packet"
XCOPY /Y ServerPacketManager.cs "../../../Server/Server/Packet"
XCOPY /Y MsgIdCache.cs "../../../Client/Assets/Scripts/Packet"
XCOPY /Y MsgIdCache.cs "../../../Server/Server/Packet"
XCOPY /Y MsgIdCache.cs "../../../Server/LoadTestClient/Packet"
XCOPY /Y ClientPacketManager.cs "../../../Server/LoadTestClient/Packet"

echo Done!
