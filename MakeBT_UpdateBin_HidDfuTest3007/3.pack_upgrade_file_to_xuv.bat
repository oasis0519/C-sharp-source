md .\psfs_pskeys 2>nul

psfscmd.exe psr2psfs ./psfs_pskeys/sys_qcc300x.psa sink_system_qcc300x.psr    

packfile ./psfs_pskeys sink_system_qcc300x.xuv

pause