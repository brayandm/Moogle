import os

def getFilesInDirectory(path):
	f = []
	for (dirpath, dirnames, filenames) in os.walk(path):
		f.extend(filenames)
		break
	return f

Files = getFilesInDirectory('content/')

for name in Files:

	if name[-4:] == ".txt":

		text = open('content/' + name, 'r').readlines()

		cad = []

		for line in text:

			for c in line:
				if c.isprintable():
					cad.append(c)
				else:
					cad.append(' ')

		
		text = open('content/' + name, 'w')

		for c in cad:
			text.write(c)

		text.close()

