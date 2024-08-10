# -*- coding: utf-8 -*-
import codecs
import re

f = codecs.open("C:\Anodyne2Repo\Assets\Resources\Dialogue\Dialogue Data_zh-simp.xml",encoding='utf-8')

# Spanish, Italian, French
charset = unicode("ÄÏÜËÖäïüëöÂÎÛÊÔâîûêôÀÌÙÈÒàìùèòÁÍÚÉÓáíúéóÃãÇçÑñõabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()-_=+[{]},<.>\'\"\\|`~/?;:¡¿ŒœßªАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя","utf-8")
i = 0
for line in f:
    print i
    i += 1
    
    for c in line:
        if c not in charset:
            charset += c

f.close()

f = codecs.open("unique_zh_simp_chars.txt",'w',encoding='utf-8')
f.write(charset)
f.close()