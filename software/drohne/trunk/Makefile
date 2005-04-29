
SOURCES = src/Main.cs

ASSEMBLIES = -r:GNU.Gettext 
MCS_OPTIONS = -debug+

all: package-messages drohne.exe

drohne.exe: $(SOURCES)
	mcs $(MCS_OPTIONS) $(ASSEMBLIES) -out:"bin/drohne.exe" -target:winexe \
	 $(SOURCES)
	cp GNU.Gettext.dll bin

package-messages:
	xgettext -ki18n $(SOURCES) -o po/drohne.pot
	
	for i in `find po -name *.po`; do \
	  lang=`basename $$i .po`; \
	  msgfmt --csharp -r i18n -l $$lang -d bin $$i; \
	done

clean:
	rm -rf bin/*
