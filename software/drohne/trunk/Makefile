SOURCES = \
  src/main.cs \
  src/gui.cs \
  src/logformat.cs

RESOURCES = \
  src/gui.glade

RESOURCES_BUILD = $(foreach res,$(RESOURCES), $(addprefix -resource:,$(res)),$(notdir $(res)))
PKG_REFERENCES_BUILD = -pkg:gtk-sharp,glade-sharp

MCS_OPTIONS = -debug+

all: drohne.exe package-messages

drohne.exe: $(SOURCES) $(RESOURCES)
	mcs $(MCS_OPTIONS) -out:"bin/drohne.exe" -target:exe \
	 $(RESOURCES_BUILD) $(PKG_REFERENCES_BUILD) $(SOURCES)

package-messages:
	xgettext -ki18n --from-code=UTF-8 $(SOURCES) $(RESOURCES) -o po/drohne.pot

clean:
	rm -rf bin/*
	-for backup in `find -name *.bak`; do rm $$backup; done
