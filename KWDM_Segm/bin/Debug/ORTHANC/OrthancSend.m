function [reply] = OrthancSend(URL, DICOMfilename)
%%% w tej wersji funkcji plik DICOM musi by� w dir, gdzie znajduje si� plik
%%% .m
%%% DICOMfilename, np. 'xd.dcm'
    execute = ['!curl -X POST' URL '/instances --data-binary @' DICOMfilename];
    reply = evalc(execute);
end

