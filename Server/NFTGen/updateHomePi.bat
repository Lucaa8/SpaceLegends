@echo off
SET KEY_PATH=C:\Users\lucad\.ssh\rpiluca
ssh -i %KEY_PATH% luca@rpi4 "screen -S token -X quit"
scp -i %KEY_PATH% app.py luca@rpi4:~/NFTGen
scp -i %KEY_PATH% Collection.py luca@rpi4:~/NFTGen
ssh -i %KEY_PATH% luca@rpi4 "cd ~/NFTGen/ && ./start.sh"
ssh -i %KEY_PATH% luca@rpi4 -t "screen -r token"