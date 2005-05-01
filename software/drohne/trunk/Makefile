SOURCES = src/Main.cs src/GUI.cs
RESOURCES = src/gui.glade

RESOURCES_BUILD = $(foreach res,$(RESOURCES), $(addprefix -resource:,$(res)),$(notdir $(res)))
PKG_REFERENCES_BUILD = -pkg:gtk-sharp,glade-sharp

CSC_OPTIONS = 

all: drohne.exe

drohne.exe: $(SOURCES) $(RESOURCES)
	mcs $(CSC_OPTIONS) -out:"bin/drohne.exe" -target:winexe \
	 $(RESOURCES_BUILD) $(PKG_REFERENCES_BUILD) $(SOURCES)

package-messages:
	xgettext -ki18n $(SOURCES) -o po/drohne.pot

clean:
	rm -rf bin/*
