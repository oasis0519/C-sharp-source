del .\*.key
del .\*.psr
::del .\*.upg
del .\*.xuv
del .\*.bin

for /d %%i in (*) do rd /s /q "%%i"
