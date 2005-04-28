FILES    = src/gpsData.cs src/guiGtk.cs src/main.cs src/i18n.cs
MCS_OPTS = -debug- -doc:doc/drohneDoc.xml -out:Drohne-gtk.exe -target:winexe
ZFILES   = doc/ share/ src/ build.bat Drohne.exe makefile README.txt
VERSION  = 0.1.0

compile:
	mcs $(MCS_OPTS) $(FILES)

clean:
	rm Drohne-gtk.exe
	rm doc/drohneDoc.xml
	rm Drohne

dist:
	-rm -r Drohne
	-rm -r drohne-$(VERSION).zip
	mkdir Drohne
	cp -r $(ZFILES) Drohne
	zip -9 -r drohne-$(VERSION).zip Drohne

# vim:fileformat=unix
