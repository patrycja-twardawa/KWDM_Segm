function [reply] = OrthancSend(URL, DICOMfilename)
%%% w tej wersji funkcji plik DICOM musi byæ w dir, gdzie znajduje siê plik
%%% .m
%%% DICOMfilename, np. 'xd.dcm'
    execute = ['!curl -X POST' URL '/instances --data-binary @' DICOMfilename];
    reply = evalc(execute);
end

