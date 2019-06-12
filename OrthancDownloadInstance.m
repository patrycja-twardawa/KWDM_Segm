function [reply] = OrthancDownloadInstance(instances)
    addpath('.\jsonlab');
    URL = 'http://localhost:8042'; %% adres serwera
    
    execute = ['!curl ' URL '/instances/' instances '/file > temp\' instances '.dcm'];
    reply = evalc(execute);
end

