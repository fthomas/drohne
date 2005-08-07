drohnedir = $(pkglibdir)
drohne_SCRIPTS = drohne.exe
CLEANFILES = drohne.exe

drohne_sources_in = AssemblyInfo.cs.in
drohne_SOURCES = gui.cs helpers.cs log.cs main.cs

drohne.exe:
	$(MCS) $(GTKSHARP_LIBS) $(GLADESHARP_LIBS) -out:$@ $<
