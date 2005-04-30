SOURCES = src/Main.cs

CSC_OPTIONS = -debug+

all: drohne.exe

drohne.exe: $(SOURCES)
	mcs $(CSC_OPTIONS) -out:"bin/drohne.exe" -target:winexe $(SOURCES)

package-messages:
	xgettext -ki18n $(SOURCES) -o po/drohne.pot

clean:
	rm -rf bin/*
