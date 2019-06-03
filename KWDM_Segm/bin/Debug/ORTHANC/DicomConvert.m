function[] = DicomConvert(instance_name)
    I = dicomread(strcat('temp\', instance_name, '.dcm'));
    
    for i = 1 : size(I,4)
        imwrite(mat2gray(I(:,:,1,i)), strcat('temp\', num2str(i), '.png'), 'png');
    end
end