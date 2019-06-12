function [] = OrthancSend(file_path, segmentation, DICOMfilename)

    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
	
	% metadane takie same
    info = dicominfo(fullfile(file_path,DICOMfilename));
    info.BodyPartExamined = [info.BodyPartExamined ' Segmentacja'];
    info.SeriesInstanceUID = '1.2.840.113619.2.55.3.2831219201.818.12178234.2';
    
    % czytaj segmentacjê
    img = imread(fullfile(file_path, segmentation));  
    dicomwrite(img, fullfile(file_path,[DICOMfilename '_segm.dcm']), info);

    % wyœlij na serwer
    execute = ['!curl -X POST ' URL '/instances --data-binary @' fullfile(file_path,[DICOMfilename '_segm.dcm'])];
    eval(execute);
end


