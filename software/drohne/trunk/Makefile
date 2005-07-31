DESTDIR =
PREFIX = /usr/local

SOURCES = \
 src/main.cs \
 src/gui.cs \
 src/helpers.cs \
 src/log.cs
    
RESOURCES = \
 src/gui.glade

RESOURCES_BUILD = $(foreach res,$(RESOURCES),$(addprefix -resource:,$(res)),$(notdir $(res)))
PKG_REFERENCES_BUILD = -pkg:gtk-sharp,glade-sharp
MCS_OPTIONS = -debug+

build: bin/drohne.exe doc/drohne.1 po/drohne.pot

bin/drohne.exe: $(SOURCES) $(RESOURCES)
	mcs $(MCS_OPTIONS) -out:"$@" -target:exe $(RESOURCES_BUILD) $(PKG_REFERENCES_BUILD) $(SOURCES)

doc/drohne.1: doc/manpage.xml
	cd doc && docbook2x-man $(notdir $<)
	gzip -c9 doc/drohne.1 > doc/drohne.1.gz

po/drohne.pot: $(SOURCES) $(RESOURCES)
	xgettext -ki18n --from-code=UTF-8 $(SOURCES) $(RESOURCES) -o $@

clean:
	rm -rf bin/drohne.exe*
	rm -f doc/drohne.1*
	-for backup in `find -name *.bak`; do rm $$backup; done

install: build
	install -D -m755 -p bin/drohne.sh $(DESTDIR)$(PREFIX)/bin/drohne
	install -D -m644 -p bin/drohne.exe $(DESTDIR)$(PREFIX)/lib/drohne/drohne.exe
	install -D -m644 -p src/drohne.desktop $(DESTDIR)$(PREFIX)/share/applications/drohne.desktop
	install -D -m644 -p doc/drohne.1.gz $(DESTDIR)$(PREFIX)/share/man/man1/drohne.1.gz
