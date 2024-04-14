@echo off
SET KEY_PATH=C:\Users\Luca\.ssh\rpiluca
ssh -i %KEY_PATH% -p 26000 luca@luca-dc.ch "screen -S token -X quit"
scp -i %KEY_PATH% -P 26000 app.py luca@luca-dc.ch:~/NFTGen
scp -i %KEY_PATH% -P 26000 Collection.py luca@luca-dc.ch:~/NFTGen
ssh -i %KEY_PATH% -p 26000 luca@luca-dc.ch "cd ~/NFTGen/ && ./start.sh"
ssh -i %KEY_PATH% -p 26000 luca@luca-dc.ch -t "screen -r token"