drohnedir = $(pkglibdir)
drohne_SCRIPTS = drohne.exe

EXTRA_DIST = $(drohne_sources)
CLEANFILES = drohne drohne.exe

drohne_sources_in = AssemblyInfo.cs.in
drohne_sources = gui.cs helpers.cs log.cs main.cs

drohne_build_sources= $(addprefix $(srcdir)/,$(drohne_sources))

drohne.exe: $(drohne_build_sources)
	$(MCS) $(GLADESHARP_LIBS) -out:$@ $(drohne_build_sources)

drohne: $(srcdir)/drohne.in
	$(REWRITE) $(srcdir)/drohne.in > $@.tmp
	mv $@.tmp $@
