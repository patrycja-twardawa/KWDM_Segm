function [] = OrthancSend(file_path, segmentation, DICOMfilename)

    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
	
	% metadane takie same
    info = dicominfo(fullfile(file_path,DICOMfilename));
    info.BodyPartExamined = [info.BodyPartExamined ' Segmentacja'];
    
    % czytaj segmentacj�
    img = imread(fullfile(file_path, segmentation));  
    dicomwrite(img, fullfile(file_path,[DICOMfilename '_segm.dcm']), info);

    % wy�lij na serwer
    execute = ['!curl -X POST ' URL '/instances --data-binary @' fullfile(file_path,[DICOMfilename '_segm.dcm'])];
    eval(execute);
end


