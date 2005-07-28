SOURCES = \
 src/main.cs \
 src/gui.cs \
 src/log.cs
    
RESOURCES = \
 src/gui.glade

build: bin/drohne.exe doc/drohne.1 po/drohne.pot

RESOURCES_BUILD = $(foreach res,$(RESOURCES),$(addprefix -resource:,$(res)),$(notdir $(res)))
PKG_REFERENCES_BUILD = -pkg:gtk-sharp,glade-sharp
MCS_OPTIONS = -debug+

bin/drohne.exe: $(SOURCES) $(RESOURCES)
	mcs $(MCS_OPTIONS) -out:"$@" -target:exe $(RESOURCES_BUILD) $(PKG_REFERENCES_BUILD) $(SOURCES)

doc/drohne.1: doc/manpage.xml
	cd doc && docbook2x-man $(notdir $<)

po/drohne.pot: $(SOURCES) $(RESOURCES)
	xgettext -ki18n --from-code=UTF-8 $(SOURCES) $(RESOURCES) -o $@

clean:
	rm -rf bin/drohne.exe*
	rm -f doc/drohne.1
	-for backup in `find -name *.bak`; do rm $$backup; done

DESTDIR =
PREFIX = /usr/local

install: bin/drohne.exe doc/drohne.1
	install -D bin/drohne.sh $(DESTDIR)$(PREFIX)/bin/drohne
	install -D bin/drohne.exe $(DESTDIR)$(PREFIX)/lib/drohne/drohne.exe
	install -D src/drohne.desktop $(DESTDIR)$(PREFIX)/share/applications/drohne.desktop
