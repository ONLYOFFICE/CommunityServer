
java -jar "compiler.jar" ^
--compilation_level ADVANCED_OPTIMIZATIONS ^
--js ^
	"ganttchart.js" ^
	--js_output_file "ganttchart_temp.js" 

	@echo off
	
	copy /b ganttchart_ver.js + ganttchart_temp.js ganttchart_min.js
	del ganttchart_temp.js

PAUSE


