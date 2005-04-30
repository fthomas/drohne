SOURCES = src/Main.cs

CSC_OPTIONS = -debug+

all: drohne.exe

drohne.exe: $(SOURCES)
	mcs $(CSC_OPTIONS) -out:"drohne.exe" -target:winexe $(SOURCES)

package-messages:
	xgettext -ki18n $(SOURCES) -o po/drohne.pot
	
	for i in `find po -name *.po`; do \
	    lang=`basename $$i .po`; \
	    msgfmt --csharp-resources $$i; \
	done

clean:
	rm -rf bin/*
