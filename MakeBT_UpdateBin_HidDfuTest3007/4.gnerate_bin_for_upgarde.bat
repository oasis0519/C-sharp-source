UpgradeFileGen CubicSpeaker_3007.upg CubicSpeaker_3007_upg.xuv

dfusign -v -f -u -h CubicSpeaker_3007_upg.xuv -o CubicSpeaker_3007_upg_signed.xuv -ka oem.private.key

xuv2bin -d CubicSpeaker_3007_upg_signed.xuv CubicSpeaker_3007_upg_signed.bin

pause