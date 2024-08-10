import codecs
import re

f = codecs.open("Dialogue Data_ru.xml",encoding='utf-8')

charset = ""
i = 0
for line in f:
    print i
    i += 1
    
    for c in line:
        if c not in charset:
            charset += c

f.close()

f = codecs.open("unique_characters_ru.txt",'w',encoding='utf-8')
f.write(charset)
f.close()
