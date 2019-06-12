function [instances] = OrthancGetInstance(URL, series)
    addpath('./jsonlab');
    instances = loadjson(urlread([ URL '/series/' series '/instances/' ]));
end