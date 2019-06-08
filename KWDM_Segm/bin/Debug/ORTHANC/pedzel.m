function [] = pedzel(file_path, wspolrzednex, wspolrzedney, obraz)
    obraz = imread([file_path obraz]);
    mask = zeros(size(obraz));
    for i = 1:length(wspolrzednex)
        obraz(round(wspolrzednex(i)), round(wspolrzedney(i))) = 1; 
        if(i>1)
           % Distance (in pixels) between the two endpoints
            nPoints = ceil(sqrt((wspolrzednex(i-1) - wspolrzednex(i)).^2 + ((wspolrzedney(i-1) - wspolrzedney(i)).^2))) + 1;

            % Determine x and y locations along the line
            xvalues = round(linspace(wspolrzednex(i-1), wspolrzednex(i), nPoints));
            yvalues = round(linspace(wspolrzedney(i-1), wspolrzedney(i), nPoints));

            % Replace the relevant values within the mask
            mask(sub2ind(size(mask), yvalues, xvalues)) = 1;
            
            [ax, by] = size(mask);
            for ix = 1:ax
                for iy = 1:by
                    if(mask(ix,iy) == 1)
                       obraz(ix,iy) = 1; 
                    end
                end
            end
        end
    end
    imwrite(obraz,fullfile(file_path,'segmCV.png'));
end
